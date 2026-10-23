namespace G_Tweaker.Core.Models;

public static class TweakCategoryExtensions
{
    public static string DisplayName(this TweakCategory category) => category switch
    {
        TweakCategory.Privacy => "Privacy",
        TweakCategory.Performance => "Performance",
        TweakCategory.Input => "Input",
        TweakCategory.Explorer => "Explorer",
        TweakCategory.Gaming => "Gaming",
        _ => category.ToString()
    };
}
