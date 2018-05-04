using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCrossTest.Core.Core.DTO;
using MvvmCrossTest.Core.Core.Services.Interfaces;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class FirstViewModel
        : MvxViewModel
    {
        private readonly ITestService m_testService;
        private readonly IMvxNavigationService m_navigationService;

        string hello = "Hello MvvmCross";
        public string Hello
        {
            get { return hello; }
            set
            {
                hello = value;
                RaisePropertyChanged(() => Hello);
                RaisePropertyChanged(() => HelloLen);
            }
        }

        public int HelloLen
        {
            get { return string.IsNullOrEmpty(Hello) ? 0 : Hello.Length; }
        }

        private string m_firstName;
        public string FirstName
        {
            get { return m_firstName; }
            set { m_firstName = value; RaisePropertyChanged(() => FirstName); }
        }

        private string m_secondName;
        public string SecondName
        {
            get { return m_secondName; }
            set { m_secondName = value; RaisePropertyChanged(() => SecondName); }
        }

        private string m_genderTitle;
        public string GenderTitle
        {
            get { return m_genderTitle; }
            set { m_genderTitle = value; RaisePropertyChanged(() => GenderTitle); }
        }

        private int m_incrementResult = 0;
        public int Increment
        {
            get { return m_incrementResult; }
            set { m_incrementResult = value; RaisePropertyChanged(() => Increment); }
        }

        private bool m_incrementLoading = false;
        public bool IncrementLoading
        {
            get { return m_incrementLoading; }
            set { m_incrementLoading = value; RaisePropertyChanged(() => IncrementLoading); }
        }

        private ICommand m_incrementCommand;
        public ICommand IncrementCommand
        {
            get
            {
                m_incrementCommand = m_incrementCommand ?? new MvxCommand(OnIncrementPressed);
                return m_incrementCommand;
            }
        }

        private ICommand m_showSecondViewCommand;
        public ICommand ShowSecondViewCommand
        {
            get
            {
                m_showSecondViewCommand = m_showSecondViewCommand ?? new MvxCommand(OnShowSecondView);
                return m_showSecondViewCommand;
            }
        }

        public FirstViewModel(ITestService testService,
                                IMvxNavigationService navigationService)
        {
            m_testService = testService;
            m_navigationService = navigationService;
        }

        public override void Start()
        {
            base.Start();

            FirstName = "Iain";
            SecondName = "Stanford";
            GenderTitle = "Mr";
        }
         
        public async void OnIncrementPressed()
        {
            if (IncrementLoading)
                return;

            IncrementLoading = true;

            var result = await m_testService.Increment();

            Increment = result;
            IncrementLoading = false;
        }

        public async void OnShowSecondView()
        {
            TestObject test = new TestObject();
            test.TestBool = true;
            test.TestString = "Testing";
            test.TestInt = 10;
            test.TestFloat = 5.0f;
            
            await m_navigationService.Navigate<SecondViewModel, TestObject>(test);
        } 
    }
}