using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCrossTest.Core.Core.DTO;
using MvvmCrossTest.Core.Core.Services.Interfaces;

namespace MvvmCrossTest.Core.Core.ViewModels
{
    public class MySettingsViewModel
        : MvxViewModel
    {
        public MySettingsViewModel()
        {
        }

        public override void Start()
        {
            base.Start();
        }

        public override Task Initialize()
        {
            return base.Initialize();
        }

        public override void Appearing()
        {
            base.Appearing();
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}