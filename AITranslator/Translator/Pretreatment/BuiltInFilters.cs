using AITranslator.Translator.Models;
using LanguageDetection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AITranslator.Translator.Pretreatment
{
    /// <summary>
    /// 源语言（当前仅支持英语、日语两种源语言，由模板文件夹推断）
    /// </summary>
    public enum SourceLanguage
    {
        None,
        English,
        Japanese,
    }

    /// <summary>
    /// 内置预处理选项：引擎数据黑名单过滤 + 源语言过滤
    /// </summary>
    public sealed class PreprocessOptions
    {
        /// <summary>
        /// 源语言（None 时不进行语言过滤）
        /// </summary>
        public SourceLanguage SourceLanguage { get; set; } = SourceLanguage.None;

        /// <summary>
        /// 是否启用引擎数据黑名单过滤
        /// </summary>
        public bool UseEngineBlacklist { get; set; } = true;

        /// <summary>
        /// 是否启用源语言过滤
        /// </summary>
        public bool UseLanguageFilter { get; set; } = true;

        /// <summary>
        /// 依据模板文件夹名称推断源语言，例如 "英文→中文" -> English、"日文→中文" -> Japanese
        /// </summary>
        public static SourceLanguage ParseSourceLanguage(string? templateDic)
        {
            if (string.IsNullOrEmpty(templateDic))
                return SourceLanguage.None;
            if (templateDic.StartsWith("英文"))
                return SourceLanguage.English;
            if (templateDic.StartsWith("日文"))
                return SourceLanguage.Japanese;
            return SourceLanguage.None;
        }
    }

    /// <summary>
    /// 引擎数据黑名单：由多个游戏的原始导出文件交叉得到的“无需翻译”内容（字体名、引擎串、控制器映射等）。
    /// 命中即视为引擎数据，不参与翻译。
    /// </summary>
    internal static class EngineBlacklist
    {
        static readonly Lazy<HashSet<string>> _set = new Lazy<HashSet<string>>(Load, LazyThreadSafetyMode.ExecutionAndPublication);

        static HashSet<string> Load()
        {
            foreach (string path in CandidatePaths())
            {
                try
                {
                    if (!File.Exists(path))
                        continue;
                    List<string>? list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
                    if (list is null)
                        continue;
                    return new HashSet<string>(list, StringComparer.Ordinal);
                }
                catch
                {
                    // 黑名单不可用不影响主流程
                }
            }
            return new HashSet<string>(StringComparer.Ordinal);
        }

        static IEnumerable<string> CandidatePaths()
        {
            yield return Path.Combine(AppContext.BaseDirectory, PublicParams.EngineBlacklistPath);
            yield return Path.GetFullPath(PublicParams.EngineBlacklistPath);
        }

        public static bool Contains(string text)
        {
            HashSet<string> set = _set.Value;
            if (set.Count == 0)
                return false;
            if (set.Contains(text))
                return true;
            string trimmed = text.Trim();
            return trimmed.Length != text.Length && set.Contains(trimmed);
        }
    }

    /// <summary>
    /// 两级源语言过滤：
    /// 第一级按字符（Unicode 区块）快速剔除其他文字体系的内容；
    /// 第二级才用语言检测库区分同一文字体系下的其他语言（如英语/葡萄牙语/西班牙语）。
    /// </summary>
    internal static class LanguageFilter
    {
        /// <summary>
        /// 低于该长度的文本不调用检测库（短文本识别不稳），仅依赖第一级字符判定
        /// </summary>
        const int MinDetectLength = 20;

        static readonly LanguageDetector _detector = CreateDetector();

        static LanguageDetector CreateDetector()
        {
            LanguageDetector detector = new LanguageDetector();
            detector.AddAllLanguages();
            return detector;
        }

        public static bool ShouldKeep(string text, SourceLanguage source)
        {
            if (source == SourceLanguage.None)
                return true;

            string value = text.Trim();
            if (value.Length == 0)
                return true;

            int latin = 0, japanese = 0, kana = 0, otherLetters = 0;
            foreach (char c in value)
            {
                if (IsLatinLetter(c))
                    latin++;
                else if (IsJapaneseChar(c))
                {
                    japanese++;
                    if (IsKana(c))
                        kana++;
                }
                else if (char.IsLetter(c))
                    otherLetters++;
            }

            switch (source)
            {
                case SourceLanguage.English:
                    // 第一级：不含拉丁字母，或被非拉丁文字主导 → 剔除
                    if (latin == 0)
                        return false;
                    if (otherLetters >= latin)
                        return false;
                    // 第二级：在拉丁字母内部区分葡/西/法等非英语
                    if (otherLetters == 0 && value.Length >= MinDetectLength)
                        return Detect(value, "eng");
                    return true;

                case SourceLanguage.Japanese:
                    // 第一级：不含任何日文字符 → 剔除
                    if (japanese == 0)
                        return false;
                    // 含假名即可判定为日语
                    if (kana > 0)
                        return true;
                    // 纯汉字：用检测库区分日语/汉语
                    if (value.Length >= MinDetectLength)
                        return Detect(value, "jpn");
                    return true;
            }
            return true;
        }

        static bool Detect(string text, string expected)
        {
            try
            {
                string? lang = _detector.Detect(text);
                if (string.IsNullOrEmpty(lang))
                    return true; // 无法判定时保守保留
                return lang == expected;
            }
            catch
            {
                return true;
            }
        }

        static bool IsLatinLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        static bool IsKana(char c)
        {
            return (c >= '\u3040' && c <= '\u309F')    // 平假名
                || (c >= '\u30A0' && c <= '\u30FF')    // 片假名
                || (c >= '\u31F0' && c <= '\u31FF');   // 片假名扩展
        }

        static bool IsJapaneseChar(char c)
        {
            return IsKana(c)
                || (c >= '\u4E00' && c <= '\u9FFF');   // CJK 汉字
        }
    }
}
