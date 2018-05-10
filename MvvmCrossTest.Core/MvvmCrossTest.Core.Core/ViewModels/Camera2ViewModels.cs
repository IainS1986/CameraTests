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

    public class Camera2PreviewViewModel : Camera2ViewModel
    {
        public Camera2PreviewViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class Camera2JNIGrayscaleViewModel : Camera2ViewModel
    {
        public Camera2JNIGrayscaleViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class Camera2JNIGrayscaleCCw90ViewModel : Camera2ViewModel
    {
        public Camera2JNIGrayscaleCCw90ViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class Camera2RGBAViewModel : Camera2ViewModel
    {
        public Camera2RGBAViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class Camera2RAWSensorViewModel : Camera2ViewModel
    {
        public Camera2RAWSensorViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }
}
