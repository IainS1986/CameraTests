using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class Camera2ViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService m_navigationService;

        public Camera2ViewModel(IMvxNavigationService navigationService)
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

    public class Camera2ToImageViewModel : Camera2ViewModel
    {
        public Camera2ToImageViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }
}
