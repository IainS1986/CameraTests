using MvvmCross.Platform.IoC;

namespace MvvmCrossTest.Core.Core
{
    public class App : MvvmCross.Core.ViewModels.MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            RegisterAppStart<ViewModels.DrawerViewModel>();
        }
    }
}
