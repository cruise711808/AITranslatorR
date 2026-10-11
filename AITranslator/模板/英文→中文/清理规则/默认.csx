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
    new Regex(@"[/\\]"),
    new Regex(@"\.(png|jpg|jpeg|webp|gif|bmp|ico|svg|ogg|opus|mp3|wav|flac|webm|mp4|avi|mkv|ttf|otf|woff2?|json|txt|csv|xml|ini|py|pyc|rpy|rpyc|rpyb|rpym|dll|exe|rpa|zip|gz)\b", RegexOptions.IgnoreCase),
    new Regex(@"==|!=|<=|>=|&&|\|\||\+=|-=|\*=|/=|:="),
    new Regex(@"="),
    new Regex(@"\w+\s*\("),
    new Regex(@"\b(persistent|renpy|store|config|gui|self)\."),
    new Regex(@"""\+|\+"""),
    new Regex(@"^\s*\$"),
    new Regex(@":\s*$"),
    new Regex(@"www\.|https?://|\b\w+\.(com|org|net|io|gg|co)\b|\b[^\s@]+@[^\s@]+\.[A-Za-z]{2,}\b"),
    new Regex(@"^[^\p{L}\p{Nd}]+$"),
    new Regex(@"^[A-Za-z0-9_]*_[A-Za-z0-9_]*$"),
    new Regex(@"^[A-Za-z0-9_]*[A-Za-z][A-Za-z0-9_]*[0-9][A-Za-z0-9_]*$"),
    new Regex(@"^[A-Za-z0-9_]*[0-9][A-Za-z0-9_]*[A-Za-z][A-Za-z0-9_]*$"),
    new Regex(@"^(?=.{6,}$)[A-Za-z0-9]*[A-Z][A-Za-z0-9]*[A-Z][A-Za-z0-9]*$"),
    new Regex(@"^(return|pass|break|continue|None|True|False|self|def|class|import|elif|try|except|finally|lambda|yield|global|assert|del|raise)$"),
};

int CountChar(string s, char c)
{
    int n = 0;
    foreach (var ch in s)
        if (ch == c)
            n++;
    return n;
}

bool IsGameCode(string s)
{
    string t = s.Trim();
    if (t.Length == 0)
        return false;
    foreach (var re in codeRegexes)
        if (re.IsMatch(t))
            return true;
    if (CountChar(t, '(') != CountChar(t, ')'))
        return true;
    if (CountChar(t, '[') != CountChar(t, ']'))
        return true;
    if (CountChar(t, '{') != CountChar(t, '}'))
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
