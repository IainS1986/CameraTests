using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class DrawerViewModel
        : MvxViewModel
    {
        readonly Type[] _menuItemTypes = {
             typeof(MyListViewModel),
             typeof(MySettingsViewModel),
             typeof(FirstViewModel),
             typeof(CameraToSurfaceTextureViewModel),
             typeof(CameraToSurfaceTextureWithCallbackViewModel),
             typeof(CameraToSurfaceTextureWithCallbackAndProcessingViewModel),
             typeof(CameraToImageViewWithCallbackAndProcessingViewModel),
             typeof(CameraToOpenGLWithCallbackAndProcessingViewModel),
             typeof(Camera2RAWViewModel),
             typeof(Camera2PreviewViewModel),
             typeof(Camera2JNIGrayscaleViewModel),
             typeof(Camera2JNIGrayscaleCCw90ViewModel),
             /*typeof(Camera2RGBAViewModel),*/
             /*typeof(Camera2RAWSensorViewModel),*/
        };

        public IEnumerable<string> MenuItems { get; private set; } = new[] {
            "My List",
            "My Settings",
            "First View",
            "Camera (Preview)",
            "Camera (Callback)",
            "Camera (Processing)",
            "Camera (Proc->Img)",
            "Camera (Proc->Opengl)",
            "Camera2RAW",
            "Camera2 (Preview)",
            "Camera2 (JNI Grayscale)",
            "Camera2 (JNI Grayscale CCw90)",
            /*"Camera2 (JNI RGBA)",*/
            /*"Camera2 (JNI RawSensor)",*/
        };

        public void ShowDefaultMenuItem()
        {
            NavigateTo(0);
        }

        public void NavigateTo(int position)
        {
            ShowViewModel(_menuItemTypes[position]);
        }
    }

    public class MenuItem : Tuple<string, Type>
    {
        public MenuItem(string displayName, Type viewModelType)
            : base(displayName, viewModelType)
        { }

        public string DisplayName
        {
            get { return Item1; }
        }

        public Type ViewModelType
        {
            get { return Item2; }
        }
    }
}
