using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCrossTest.Core.Core.DTO;
using MvvmCrossTest.Core.Core.Services.Interfaces;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class Camera2RAWViewModel
        : MvxViewModel
    {
        private readonly IMvxNavigationService m_navigationService;

        public Camera2RAWViewModel(IMvxNavigationService navigationService)
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
    }
}