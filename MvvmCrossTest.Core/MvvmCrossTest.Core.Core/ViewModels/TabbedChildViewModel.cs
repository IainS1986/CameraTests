using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class TabbedChildViewModel 
        : MvxViewModel
    {
        private string m_tabTitle;
        public string TabTitle
        {
            get { return m_tabTitle; }
            set
            {
                m_tabTitle = value;
                RaisePropertyChanged(() => TabTitle);
            }
        }

        public TabbedChildViewModel(string tab = "default")
        {
            TabTitle = tab;
        }
    }

}
