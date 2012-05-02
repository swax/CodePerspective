using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace XLibrary
{
    public interface IColorProfile
    {
        Color BackgroundColor { get; }

        Color UnknownColor { get; }
        Color FileColor { get; }
        Color NamespaceColor { get; }
        Color ClassColor { get; }
        Color MethodColor { get; }
        Color FieldColor { get; }

        Color EmptyColor { get; }
        Color OutsideColor { get; }

        Color EntryColor { get; }
        Color MultiEntryColor { get; }

        Color HoldingColor { get; }
        Color MultiHoldingColor { get; }

        Color SearchMatchColor { get; }

        Color CallColor { get; }
        Color ShowCallColor { get; }
        Color CallOutColor { get; }
        Color CallInColor { get; }
        Color CallOutColorFocused { get; }
        Color CallInColorFocused { get; }
        Color HoldingCallColor { get; }

        Color BorderColor { get; }
        Color CallDividerColor { get; }

        Color HitColor { get; }
        Color MultiHitColor { get; }

        Color ExceptionColor { get; }
        Color MultiExceptionColor { get; }

        Color FieldSetColor { get; }
        Color FieldGetColor { get; }

        Color TextColor { get; }
        Color TextBgColor { get; }
        Color LabelBgColor { get; }
        Color FooterBgColor { get; }

        Color InstanceColor { get; }

        Color FilteredColor { get; }
        Color IgnoredColor { get; }

        Color DependentColor { get; }
        Color IndependentColor { get; }
        Color InterdependentColor { get; }
    }

    public static class IColorExtensions
    {
         public static Color GetColorForType(this IColorProfile profile, XObjType type)
         {
             switch(type)
             {
                 case XObjType.Class:
                     return profile.ClassColor;
                 case XObjType.Field:
                     return profile.FieldColor;
                 case XObjType.File:
                     return profile.FileColor;
                 case XObjType.Method:
                     return profile.MethodColor;
                 case XObjType.Namespace:
                     return profile.NamespaceColor;
                 default:
                     return profile.UnknownColor;
             }
         }
    }
   

    public class BrightColorProfile : IColorProfile
    {
        public Color BackgroundColor { get { return Color.White; } }

        public Color UnknownColor { get { return Color.Black; } }
        public Color FileColor { get { return Color.Black; } }
        public Color NamespaceColor { get { return Color.DarkBlue; } }
        public Color ClassColor { get { return Color.DarkGreen; } }
        public Color MethodColor { get { return Color.DarkRed; } }
        public Color FieldColor { get { return Color.Goldenrod; } }

        public Color EmptyColor { get { return Color.White; } }
        public Color OutsideColor { get { return Color.LightGray; } }

        public Color EntryColor { get { return Color.LightGreen; } }
        public Color MultiEntryColor { get { return Color.LimeGreen; } }

        public Color HoldingColor { get { return Color.FromArgb(255, 255, 192); } }
        public Color MultiHoldingColor { get { return Color.Yellow; } }

        public Color SearchMatchColor { get { return Color.Red; } }

        public Color CallColor { get { return Color.DarkGreen; } }
        public Color ShowCallColor { get { return Color.FromArgb(32, Color.Black); } }
        public Color CallOutColor { get { return Color.FromArgb(48, Color.Red); } }
        public Color CallInColor { get { return Color.FromArgb(48, Color.Blue); } }
        public Color CallOutColorFocused { get { return Color.FromArgb(70, Color.Red); } }
        public Color CallInColorFocused { get { return Color.FromArgb(70, Color.Blue); } }
        public Color HoldingCallColor { get { return Color.FromArgb(48, Color.Green); } }

        public Color BorderColor { get { return Color.Silver; } }
        public Color CallDividerColor { get { return Color.FromArgb(0xcc, 0xcc, 0xcc); } }

        public Color HitColor { get { return Color.FromArgb(255, 192, 128); } }
        public Color MultiHitColor { get { return Color.Orange; } }

        public Color ExceptionColor { get { return Color.Red; } }
        public Color MultiExceptionColor { get { return Color.DarkRed; } }

        public Color FieldSetColor { get { return Color.Blue; } }
        public Color FieldGetColor { get { return Color.LimeGreen; } }

        public Color TextColor { get { return Color.Black; } }
        public Color TextBgColor { get { return Color.FromArgb(192, Color.White); } }
        public Color LabelBgColor { get { return Color.FromArgb(128, Color.White); } }
        public Color FooterBgColor { get { return Color.White; } }

        public Color InstanceColor { get { return Color.Black; } }

        public Color FilteredColor { get { return Color.LimeGreen; } }
        public Color IgnoredColor { get { return Color.Red; } }

        public Color DependentColor { get { return Color.Red; } }
        public Color IndependentColor { get { return Color.Blue; } }
        public Color InterdependentColor { get { return Color.Purple; } }
    }

    public class DeusExColorProfile : IColorProfile
    {
        public Color BackgroundColor { get { return Color.Black; } }

        public Color UnknownColor { get { return Color.Yellow; } }
        public Color FileColor { get { return Color.Yellow; } }
        public Color NamespaceColor { get { return Color.Yellow; } }
        public Color ClassColor { get { return Color.Yellow; } }
        public Color MethodColor { get { return Color.Yellow; } }
        public Color FieldColor { get { return Color.Yellow; } }

        public Color EmptyColor { get { return Color.Black; } }
        public Color OutsideColor { get { return Color.LightGray; } }

        public Color EntryColor { get { return Color.LightGreen; } }
        public Color MultiEntryColor { get { return Color.LimeGreen; } }

        public Color HoldingColor { get { return Color.FromArgb(255, 255, 192); } }
        public Color MultiHoldingColor { get { return Color.Yellow; } }

        public Color SearchMatchColor { get { return Color.Red; } }

        public Color CallColor { get { return Color.LimeGreen; } }
        public Color ShowCallColor { get { return Color.FromArgb(32, Color.White); } }
        public Color CallOutColor { get { return Color.FromArgb(48, Color.Orange); } }
        public Color CallInColor { get { return Color.FromArgb(48, Color.Red); } }
        public Color CallOutColorFocused { get { return Color.FromArgb(70, Color.Orange); } }
        public Color CallInColorFocused { get { return Color.FromArgb(70, Color.Red); } }
        public Color HoldingCallColor { get { return Color.FromArgb(48, Color.Green); } }

        public Color BorderColor { get { return Color.Silver; } }
        public Color CallDividerColor { get { return Color.FromArgb(0xcc, 0xcc, 0xcc); } }

        public Color HitColor { get { return Color.FromArgb(255, 192, 128); } }
        public Color MultiHitColor { get { return Color.Orange; } }

        public Color ExceptionColor { get { return Color.Red; } }
        public Color MultiExceptionColor { get { return Color.DarkRed; } }

        public Color FieldSetColor { get { return Color.Red; } }
        public Color FieldGetColor { get { return Color.LimeGreen; } }

        public Color TextColor { get { return Color.White; } }
        public Color TextBgColor { get { return Color.FromArgb(192, Color.Black); } }
        public Color LabelBgColor { get { return Color.FromArgb(128, Color.Black); } }
        public Color FooterBgColor { get { return Color.Black; } }

        public Color InstanceColor { get { return Color.White; } }

        public Color FilteredColor { get { return Color.LimeGreen; } }
        public Color IgnoredColor { get { return Color.Red; } }

        public Color DependentColor { get { return Color.Red; } }
        public Color IndependentColor { get { return Color.Yellow; } }
        public Color InterdependentColor { get { return Color.Orange; } }
    }
}
