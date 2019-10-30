using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 百度经验个人助手
{
    public static class StatManager
    {

        private static ObservableCollection<DataPack> _dataPacksSelected;
        public static ObservableCollection<DataPack> DataPacksSelected //ObservableCollection支持排序
        {
            get { return _dataPacksSelected; }
            set
            {
                if (value != null && value.Count > 0)
                {
                    _dataPacksSelected = new ObservableCollection<DataPack>(value.OrderBy(t => t.date));
                    
                }   
            }
        }

        public static DataPack LastDateDataPack //BUG To Fix TODO:
        {
            get
            {
                if (_dataPacksSelected.Count > 0)
                    return _dataPacksSelected.Last();

                return null;
            }
        }

        //需要总共datapack计数和索引
        //需要构建主数据绘图数据
        //每篇经验采用什么方式呢？ExpName做键值（低耦合）
        //单篇浏览量趋势
        //总体趋势
        //统计信息是否可用标志位
        private static ObservableCollection<ContentExpEntry> _deltaExps;

        public static ObservableCollection<ContentExpEntry> DeltaExps
        {
            get { return _deltaExps; }
        }

        public static float _progress;

        public static float Progress
        {
            get { return _progress; }
        }

        public static void Init()
        {
            _deltaExps = new ObservableCollection<ContentExpEntry>();
        }

        public static async Task Calc(
            ObservableCollection<ContentExpEntry> expsOld,
            ObservableCollection<ContentExpEntry> expsNew)
        {
            _expsNew = expsNew;
            _expsOld = expsOld;
            CalcDeltaExpsDelegate d = CalcDeltaExps;
            await Task.Run(new Action(d));
        }


        private static ObservableCollection<ContentExpEntry> _expsOld;
        private static ObservableCollection<ContentExpEntry> _expsNew;
        private delegate void CalcDeltaExpsDelegate();

        private static void CalcDeltaExps()
        {
            _deltaExps.Clear();
            _progress = 0;
            float len = _expsOld.Count;
            int count = 0;
            foreach (ContentExpEntry ets in _expsOld)   //对于历史数据中的每个经验条目
            {
                IEnumerable<ContentExpEntry> ic = _expsNew.Where(t => t.ExpName == ets.ExpName && t.Date == ets.Date); //寻找经验名称相等的条目
                if (ic.Any())
                {
                    string shortName;
                    ContentExpEntry ft = ic.First();    //ft即所寻获的条目
                    if (ft.ExpName.Length > 10)
                        shortName = ft.ExpName.Substring(0, 10) + "...";
                    else shortName = ft.ExpName;
                    ContentExpEntry eet = new ContentExpEntry(shortName, "", ft.View - ets.View, ft.Vote - ets.Vote, 0, ets.Date);  //浏览量做差
                    _deltaExps.Add(eet);
                    ft.ViewIncrease = ft.View - ets.View; //浏览量做差，保存
                }
                count++;
                _progress = count / len;
            }
        }
    }
}
