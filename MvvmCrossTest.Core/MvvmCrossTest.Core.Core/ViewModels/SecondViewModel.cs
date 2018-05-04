using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCrossTest.Core.Core.DTO;
using MvvmCrossTest.Core.Core.Services.Interfaces;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class SecondViewModel
        : MvxViewModel, IMvxViewModel<TestObject>
    {
        private readonly IMvxNavigationService m_navigationService;

        private TestObject m_input;
        public TestObject Input
        {
            get { return m_input; }
            set { m_input = value; }
        }

        private ICommand m_showTabbedViewModelCommand;
        public ICommand ShowTabbedViewCommand
        {
            get
            {
                m_showTabbedViewModelCommand = m_showTabbedViewModelCommand ?? new MvxCommand(OnShowTabbedView);
                return m_showTabbedViewModelCommand;
            }
        }

        public SecondViewModel(IMvxNavigationService navigationService)
        {
            m_navigationService = navigationService;
        }

        public override void Start()
        {
            base.Start();
        }

        public override Task Initialize()
        {
            return base.Initialize();
        }

        public Task Initialize(TestObject parameter)
        {
            // receive and store the parameter here
            Input = parameter;

            return base.Initialize();
        }

        public async void OnShowTabbedView()
        {
            await m_navigationService.Navigate<TabbedViewModel>();
        }
    }
}