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
        Color HoldingCallColor { get; }

        Color BorderColor { get; }
        Color CallDividerColor { get; }

        Color HitColor { get; }
        Color MultiHitColor { get; }

        Color ConstructedColor { get; }
        Color DisposedColor { get; }

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

        Color[] OverColors { get; }
        Color[] HitColors { get; }
        Color[] MultiHitColors { get; }

        Color[] ConstructedColors { get; }
        Color[] DisposedColors { get; }

        Color[] FieldSetColors { get; }
        Color[] FieldGetColors { get; }

        Color[] ExceptionColors { get; }

        Color[] CallPenColors { get; }

        Color[] ObjColors { get; }
        Color[] ObjDitheredColors { get; }
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
        public Color CallOutColor { get { return Color.FromArgb(60, Color.Red); } }
        public Color CallInColor { get { return Color.FromArgb(60, Color.Blue); } }
        public Color HoldingCallColor { get { return Color.FromArgb(48, Color.Green); } }

        public Color BorderColor { get { return Color.Silver; } }
        public Color CallDividerColor { get { return Color.FromArgb(0xcc, 0xcc, 0xcc); } }

        public Color HitColor { get { return Color.Orange; } }
        public Color MultiHitColor { get { return Color.Orange; } }

        public Color ConstructedColor { get { return Color.Green; } }
        public Color DisposedColor { get { return Color.Red; } }

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

        Color[] _OverColors;
        public Color[] OverColors { get { return _OverColors; } }

        Color[] _HitColors;
        public Color[] HitColors { get { return _HitColors; } }

        Color[] _MultiHitColors;
        public Color[] MultiHitColors { get { return _MultiHitColors; } }

        Color[] _ConstructedColors;
        public Color[] ConstructedColors { get { return _ConstructedColors; } }

        Color[] _DisposedColors;
        public Color[] DisposedColors { get { return _DisposedColors; } }

        Color[] _FieldSetColors;
        public Color[] FieldSetColors { get { return _FieldSetColors; } }

        Color[] _FieldGetColors;
        public Color[] FieldGetColors { get { return _FieldGetColors; } }

        Color[] _ExceptionColors;
        public Color[] ExceptionColors { get { return _ExceptionColors; } }

        Color[] _CallPenColors;
        public Color[] CallPenColors { get { return _CallPenColors; } }

        public Color[] _ObjColors;
        public Color[] ObjColors { get { return _ObjColors; } }

        public Color[] _ObjDitheredColors;
        public Color[] ObjDitheredColors { get { return _ObjDitheredColors; } }
        

        public BrightColorProfile()
        {
            _OverColors = new Color[7];
            _HitColors = new Color[XRay.HitFrames];
            _MultiHitColors = new Color[XRay.HitFrames];

            _ExceptionColors = new Color[XRay.HitFrames];
            _FieldSetColors = new Color[XRay.HitFrames];
            _FieldGetColors = new Color[XRay.HitFrames];
            _ConstructedColors = new Color[XRay.HitFrames];
            _DisposedColors = new Color[XRay.HitFrames];

            _CallPenColors = new Color[XRay.HitFrames];

            for (int i = 0; i < _OverColors.Length; i++)
            {
                int brightness = 128 / (_OverColors.Length + 1) * (_OverColors.Length - i);
                _OverColors[i] = Color.FromArgb(128 + brightness, 128 + brightness, 255);
            }

            for (int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 - (255 / XRay.HitFrames * i);

                _HitColors[i] = Color.FromArgb(255 - brightness, HitColor);
                _MultiHitColors[i] = Color.FromArgb(255 - brightness, MultiHitColor);
                _ExceptionColors[i] = Color.FromArgb(255 - brightness, ExceptionColor);

                _ConstructedColors[i] = Color.FromArgb(255 - brightness, ConstructedColor);
                _DisposedColors[i] = Color.FromArgb(255 - brightness, DisposedColor);

                _FieldSetColors[i] = Color.FromArgb(255 - brightness, FieldSetColor);
                _FieldGetColors[i] = Color.FromArgb(255 - brightness, FieldGetColor);

                _CallPenColors[i] = Color.FromArgb(255 - brightness, CallColor);
            }

            var objTypes = Enum.GetValues(typeof(XObjType));

            _ObjColors = new Color[objTypes.Length];
            _ObjDitheredColors = new Color[objTypes.Length];

            _ObjColors[(int)XObjType.Root] = UnknownColor;
            _ObjColors[(int)XObjType.External] = UnknownColor;
            _ObjColors[(int)XObjType.Internal] = UnknownColor;
            _ObjColors[(int)XObjType.File] = FileColor;
            _ObjColors[(int)XObjType.Namespace] = NamespaceColor;
            _ObjColors[(int)XObjType.Class] = ClassColor;
            _ObjColors[(int)XObjType.Field] = FieldColor;
            _ObjColors[(int)XObjType.Method] = MethodColor;

            for (int i = 0; i < objTypes.Length; i++)
                _ObjDitheredColors[i] = Color.FromArgb(128, ObjColors[i]);
        }
    }


    public class GibsonColorProfile : IColorProfile
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
        public Color CallOutColor { get { return Color.FromArgb(60, Color.Red); } }
        public Color CallInColor { get { return Color.FromArgb(60, Color.Blue); } }
        public Color HoldingCallColor { get { return Color.FromArgb(48, Color.Green); } }

        public Color BorderColor { get { return Color.Silver; } }
        public Color CallDividerColor { get { return Color.FromArgb(0xcc, 0xcc, 0xcc); } }

        public Color HitColor { get { return Color.FromArgb(255, 192, 128); } }
        public Color MultiHitColor { get { return Color.Orange; } }

        public Color ConstructedColor { get { return Color.Green; } }
        public Color DisposedColor { get { return Color.Red; } }

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

        Color[] _OverColors;
        public Color[] OverColors { get { return _OverColors; } }

        Color[] _HitColors;
        public Color[] HitColors { get { return _HitColors; } }

        Color[] _MultiHitColors;
        public Color[] MultiHitColors { get { return _MultiHitColors; } }

        Color[] _ConstructedColors;
        public Color[] ConstructedColors { get { return _ConstructedColors; } }

        Color[] _DisposedColors;
        public Color[] DisposedColors { get { return _DisposedColors; } }

        Color[] _FieldSetColors;
        public Color[] FieldSetColors { get { return _FieldSetColors; } }

        Color[] _FieldGetColors;
        public Color[] FieldGetColors { get { return _FieldGetColors; } }

        Color[] _ExceptionColors;
        public Color[] ExceptionColors { get { return _ExceptionColors; } }

        Color[] _CallPenColors;
        public Color[] CallPenColors { get { return _CallPenColors; } }

        public Color[] _ObjColors;
        public Color[] ObjColors { get { return _ObjColors; } }

        public Color[] _ObjDitheredColors;
        public Color[] ObjDitheredColors { get { return _ObjDitheredColors; } }


        public GibsonColorProfile()
        {
            _OverColors = new Color[7];
            _HitColors = new Color[XRay.HitFrames];
            _MultiHitColors = new Color[XRay.HitFrames];

            _ExceptionColors = new Color[XRay.HitFrames];
            _FieldSetColors = new Color[XRay.HitFrames];
            _FieldGetColors = new Color[XRay.HitFrames];
            _ConstructedColors = new Color[XRay.HitFrames];
            _DisposedColors = new Color[XRay.HitFrames];

            _CallPenColors = new Color[XRay.HitFrames];

            for (int i = 0; i < _OverColors.Length; i++)
            {
                int brightness = 128 / (_OverColors.Length + 1) * (_OverColors.Length - i);
                _OverColors[i] = Color.FromArgb(128 + brightness, 128 + brightness, 255);
            }

            for (int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 - (255 / XRay.HitFrames * i);

                _HitColors[i] = Color.FromArgb(255 - brightness, HitColor);
                _MultiHitColors[i] = Color.FromArgb(255 - brightness, MultiHitColor);
                _ExceptionColors[i] = Color.FromArgb(255 - brightness, ExceptionColor);

                _ConstructedColors[i] = Color.FromArgb(255 - brightness, ConstructedColor);
                _DisposedColors[i] = Color.FromArgb(255 - brightness, DisposedColor);

                _FieldSetColors[i] = Color.FromArgb(255 - brightness, FieldSetColor);
                _FieldGetColors[i] = Color.FromArgb(255 - brightness, FieldGetColor);

                _CallPenColors[i] = Color.FromArgb(255 - brightness, CallColor);
            }

            var objTypes = Enum.GetValues(typeof(XObjType));

            _ObjColors = new Color[objTypes.Length];
            _ObjDitheredColors = new Color[objTypes.Length];

            _ObjColors[(int)XObjType.Root] = UnknownColor;
            _ObjColors[(int)XObjType.External] = UnknownColor;
            _ObjColors[(int)XObjType.Internal] = UnknownColor;
            _ObjColors[(int)XObjType.File] = FileColor;
            _ObjColors[(int)XObjType.Namespace] = NamespaceColor;
            _ObjColors[(int)XObjType.Class] = ClassColor;
            _ObjColors[(int)XObjType.Field] = FieldColor;
            _ObjColors[(int)XObjType.Method] = MethodColor;

            for (int i = 0; i < objTypes.Length; i++)
                _ObjDitheredColors[i] = Color.FromArgb(128, ObjColors[i]);
        }
    }

    public class NightColorProfile : IColorProfile
    {
        public Color BackgroundColor { get { return Color.Black; } }

        public Color UnknownColor { get { return Color.Purple; } }
        public Color FileColor { get { return Color.Purple; } }
        public Color NamespaceColor { get { return Color.Cyan; } }
        public Color ClassColor { get { return Color.Green; } }
        public Color MethodColor { get { return Color.Red; } }
        public Color FieldColor { get { return Color.Yellow; } }

        public Color EmptyColor { get { return Color.Black; } }
        public Color OutsideColor { get { return Color.DarkGray; } }

        public Color EntryColor { get { return Color.LightGreen; } }
        public Color MultiEntryColor { get { return Color.LimeGreen; } }

        public Color HoldingColor { get { return Color.FromArgb(255, 255, 192); } }
        public Color MultiHoldingColor { get { return Color.Yellow; } }

        public Color SearchMatchColor { get { return Color.Red; } }

        public Color CallColor { get { return Color.DarkGreen; } }
        public Color ShowCallColor { get { return Color.FromArgb(32, Color.Black); } }
        public Color CallOutColor { get { return Color.FromArgb(60, Color.Red); } }
        public Color CallInColor { get { return Color.FromArgb(60, Color.Blue); } }
        public Color HoldingCallColor { get { return Color.FromArgb(48, Color.Green); } }

        public Color BorderColor { get { return Color.Silver; } }
        public Color CallDividerColor { get { return Color.FromArgb(0xcc, 0xcc, 0xcc); } }

        public Color HitColor { get { return Color.FromArgb(255, 192, 128); } }
        public Color MultiHitColor { get { return Color.Orange; } }

        public Color ConstructedColor { get { return Color.Green; } }
        public Color DisposedColor { get { return Color.Red; } }

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

        Color[] _OverColors;
        public Color[] OverColors { get { return _OverColors; } }

        Color[] _HitColors;
        public Color[] HitColors { get { return _HitColors; } }

        Color[] _MultiHitColors;
        public Color[] MultiHitColors { get { return _MultiHitColors; } }

        Color[] _ConstructedColors;
        public Color[] ConstructedColors { get { return _ConstructedColors; } }

        Color[] _DisposedColors;
        public Color[] DisposedColors { get { return _DisposedColors; } }

        Color[] _FieldSetColors;
        public Color[] FieldSetColors { get { return _FieldSetColors; } }

        Color[] _FieldGetColors;
        public Color[] FieldGetColors { get { return _FieldGetColors; } }

        Color[] _ExceptionColors;
        public Color[] ExceptionColors { get { return _ExceptionColors; } }

        Color[] _CallPenColors;
        public Color[] CallPenColors { get { return _CallPenColors; } }

        public Color[] _ObjColors;
        public Color[] ObjColors { get { return _ObjColors; } }

        public Color[] _ObjDitheredColors;
        public Color[] ObjDitheredColors { get { return _ObjDitheredColors; } }
        

        public NightColorProfile()
        {
            _OverColors = new Color[7];
            _HitColors = new Color[XRay.HitFrames];
            _MultiHitColors = new Color[XRay.HitFrames];

            _ExceptionColors = new Color[XRay.HitFrames];
            _FieldSetColors = new Color[XRay.HitFrames];
            _FieldGetColors = new Color[XRay.HitFrames];
            _ConstructedColors = new Color[XRay.HitFrames];
            _DisposedColors = new Color[XRay.HitFrames];

            _CallPenColors = new Color[XRay.HitFrames];

            for (int i = 0; i < _OverColors.Length; i++)
            {
                int brightness = 128 / (_OverColors.Length + 1) * (_OverColors.Length - i);
                _OverColors[i] = Color.FromArgb(128 + brightness, 128 + brightness, 255);
            }

            for (int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 - (255 / XRay.HitFrames * i);

                _HitColors[i] = Color.FromArgb(255 - brightness, HitColor);
                _MultiHitColors[i] = Color.FromArgb(255 - brightness, MultiHitColor);
                _ExceptionColors[i] = Color.FromArgb(255 - brightness, ExceptionColor);

                _ConstructedColors[i] = Color.FromArgb(255 - brightness, ConstructedColor);
                _DisposedColors[i] = Color.FromArgb(255 - brightness, DisposedColor);

                _FieldSetColors[i] = Color.FromArgb(255 - brightness, FieldSetColor);
                _FieldGetColors[i] = Color.FromArgb(255 - brightness, FieldGetColor);

                _CallPenColors[i] = Color.FromArgb(255 - brightness, CallColor);
            }

            var objTypes = Enum.GetValues(typeof(XObjType));

            _ObjColors = new Color[objTypes.Length];
            _ObjDitheredColors = new Color[objTypes.Length];

            _ObjColors[(int)XObjType.Root] = UnknownColor;
            _ObjColors[(int)XObjType.External] = UnknownColor;
            _ObjColors[(int)XObjType.Internal] = UnknownColor;
            _ObjColors[(int)XObjType.File] = FileColor;
            _ObjColors[(int)XObjType.Namespace] = NamespaceColor;
            _ObjColors[(int)XObjType.Class] = ClassColor;
            _ObjColors[(int)XObjType.Field] = FieldColor;
            _ObjColors[(int)XObjType.Method] = MethodColor;

            for (int i = 0; i < objTypes.Length; i++)
                _ObjDitheredColors[i] = Color.FromArgb(128, ObjColors[i]);
        }
    }   
}
