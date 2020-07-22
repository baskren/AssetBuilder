using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetBuilder
{
    public class ButtonEffect : Xamarin.Forms.RoutingEffect
    {
        protected ButtonEffect() : base("AssetBuilder."+nameof(ButtonEffect))
        {
        }

        public static void ApplyTo(Xamarin.Forms.Button button)
        {
            if (button?.Effects is IList<Xamarin.Forms.Effect> effects && !effects.Any(e=>e.GetType() == typeof(ButtonEffect)))
            {
                button.Effects.Add(new ButtonEffect());


            }
        }
    }
}
