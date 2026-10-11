//Strs是脚本输入的变量,一个List<string> 类型的数据
//需要返回的也是一个List<string>，这个List里包含的是需要被保留的数据

using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 校验英语用的正则表达式
/// </summary>
Dictionary<Regex, bool> regexes = new Dictionary<Regex, bool>()
{
    { new Regex(@"^\d+$"),true },  // 匹配仅包含数字
    { new Regex(@"^[\d,]+$"),true },  // 匹配仅包含数字和逗号
    { new Regex(@"^[+\-*/=]+$"),true},  // 匹配仅包含运算符号
    { new Regex(@"^[,.;_*+/=]+$"),true },  // 匹配仅包含标点字符的组合
};

// —— 游戏代码/资源/标识符过滤：命中即剔除（不加入返回列表）——
Regex[] codeRegexes = new Regex[]
{
    new Regex(@"\w+\s*\("),                                                          // 函数调用 name(
    new Regex(@"==|!=|<=|>=|&&|\|\||\+=|-=|\*=|/=|:="),                              // 代码运算符
    new Regex(@"\b(persistent|renpy|store|config|gui)\.", RegexOptions.IgnoreCase),  // 引擎命名空间
    new Regex(@"""\+|\+"""),                                                         // 字符串拼接/插值
    new Regex(@"\s=\s|^\s*\$"),                                                      // 赋值 / Ren'Py $ python 行
    new Regex(@"\.(png|jpg|jpeg|webp|gif|bmp|ogg|opus|mp3|wav|flac|webm|mp4|avi|mkv|ttf|otf|woff2?|json|txt|csv|xml|ini|py|rpy|rpyc|pyc|dll|exe|rpa|rpym|rpyb)\b", RegexOptions.IgnoreCase), // 资源扩展名
    new Regex(@"[/\\]"),                                                             // 路径分隔符
    new Regex(@"^[A-Za-z0-9_]*_[A-Za-z0-9_]*$"),                                     // 变量标识符 __main__ / D12JadeAlt_28
};

bool IsGameCode(string s)
{
    foreach (var re in codeRegexes)
        if (re.IsMatch(s))
            return true;
    return false;
}

List<string> output = new List<string>();

foreach (var Str in Strs)
{
    if (IsGameCode(Str))
        continue;

    bool needAdd = true;
    foreach (var regexkv in regexes)
    {
        Regex regex = regexkv.Key;
        bool reverse = regexkv.Value;
        bool result = regex.IsMatch(Str);
        if (!reverse)
            result = !result;
        if (result)
        {
            needAdd = false;
            break;
        }
    }

    if (needAdd)
        output.Add(Str);
}

return output;
