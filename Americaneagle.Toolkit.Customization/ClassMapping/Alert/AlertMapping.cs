using CMS.DataEngine;
using Migration.Tool.Common.Builders;

namespace Americaneagle.Toolkit.Customization.ClassMapping.Alert;

public class AlertMapping(string targetClassName, Action<DataClassInfo> classPatcher) : MultiClassMapping(targetClassName, classPatcher)
{
    
}
