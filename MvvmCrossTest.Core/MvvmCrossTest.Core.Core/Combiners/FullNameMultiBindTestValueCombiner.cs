using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.Combiners
{
    public class FullNameMultiBindTestValueCombiner : MvxValueCombiner
    {
        public override Type SourceType(IEnumerable<IMvxSourceStep> steps)
        {
            return typeof(string);
        }

        public override bool TryGetValue(IEnumerable<IMvxSourceStep> steps, out object value)
        {
            if (steps == null || steps.Count() != 3)
            {
                value = "Failed";
                return false;
            }

            value = string.Format("{0}. {1} {2}", steps.Select(x => x.GetValue().ToString()).ToArray());

            return true;
        }
    }
}
