using System;
using System.Collections.Generic;
using System.Linq;
using WheelWizard.Models.Enums;
using WheelWizard.Views.Components.WhWzLibrary;

namespace WheelWizard.Utilities;

public static class BadgeManager
{
    public static BadgeVariant[] GetRandomBadgeSample(int? seed = null)
    {
        var random = seed == null ? new Random() : new Random(seed.Value);
        var allVariants = Enum.GetValues(typeof(BadgeVariant)) as BadgeVariant[];
        var numberOfBadges = random.Next(0, 5);

        var selectedVariants = new List<BadgeVariant>();
        for (var i = 0; i < numberOfBadges; i++)
        {
            if (allVariants == null) continue;
            var randomIndex = random.Next(allVariants.Length);
            selectedVariants.Add(allVariants[randomIndex]);
        }

        return selectedVariants.ToArray();
    }
    
    public static BadgeVariant[] GetBadgeVariants(string friendCode)
    {
        return GetRandomBadgeSample();
    }
    
    public static IEnumerable<Badge> GetBadges(string friendCode)
    {
        var badgeVariants = GetBadgeVariants(friendCode);
        return badgeVariants.Select(variant => new Badge { Variant = variant });
    }
}
