﻿using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class CameraViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService m_navigationService;

        public CameraViewModel(IMvxNavigationService navigationService)
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

    public class CameraToSurfaceTextureViewModel : CameraViewModel
    {
        public CameraToSurfaceTextureViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class CameraToSurfaceTextureWithCallbackViewModel : CameraViewModel
    {
        public CameraToSurfaceTextureWithCallbackViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class CameraToSurfaceTextureWithCallbackAndProcessingViewModel : CameraViewModel
    {
        public CameraToSurfaceTextureWithCallbackAndProcessingViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class CameraToImageViewWithCallbackAndProcessingViewModel : CameraViewModel
    {
        public CameraToImageViewWithCallbackAndProcessingViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }

    public class CameraToOpenGLWithCallbackAndProcessingViewModel : CameraViewModel
    {
        public CameraToOpenGLWithCallbackAndProcessingViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }
    }
}
