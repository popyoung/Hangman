using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HugeHard;
using HugeHard.Json;

namespace Hangman
{
    public partial class Form1
    {
        readonly JsonHelper<JsonUserConfig, Form1> JsonConfigHelper;
        JsonUserConfig Config => JsonConfigHelper.Config;
        public class JsonUserConfig : JsonConfig<Form1>
        {
            public string 词库 = "";
            public int 剩余次数 = 10;
            public int 成功次数 = 0;
            public int 失败次数 = 0;
            public DictionaryEx<string, int> 单词权重 = new(() => DEFAULT_WEIGHT);
            public DictionaryEx<string, string> 单词提示 = new(() => "null");
        }
    }
}
