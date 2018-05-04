using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCrossTest.Core.Core.DTO;
using MvvmCrossTest.Core.Core.Services.Interfaces;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class TabbedViewModel
        : MvxViewModel
    {

        private List<TabbedChildViewModel> m_tabs;
        public List<TabbedChildViewModel> Tabs
        {
            get { return m_tabs; }
            set
            {
                m_tabs = value;
                RaisePropertyChanged(() => Tabs);
            }
        }

        public TabbedViewModel()
        {
        }

        public override void Start()
        {
            base.Start();

            Tabs = new List<TabbedChildViewModel>()
            {
                new TabbedChildViewModel("First"),
                new TabbedChildViewModel("Second"),
                new TabbedChildViewModel("Third")
            };
        }
    }
}