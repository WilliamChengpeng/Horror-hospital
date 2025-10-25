using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VolFx
{
    [CreateAssetMenu(menuName = "2D/Color Qualifier", fileName = "ColorQualifierAsset", order = 0)]
    public class ColorQualifierAsset : ScriptableObject
    {
        public  const int s_LutSize = 16;
        
        public  List<ColorQualifier> _samples;
        private Texture2D            _lut; // contains color mask backed in lut table
        public  Texture2D            Lut
        {
            get
            {
                if (_lut == null)
                    OnValidate();
                
                return _lut;
            }
        }

        // =======================================================================
        private void OnValidate()
        {
            _lut = new Texture2D(s_LutSize * s_LutSize, s_LutSize, TextureFormat.RGBA32, false, true);
            for (var x = 0; x < s_LutSize * s_LutSize; x++)
            for (var y = 0; y < s_LutSize; y++)
            {
                var isMet = _samples.Any(n => n.IsMet(_lutAt(x, y)));
                _lut.SetPixel(x, y, isMet ? Color.white : Color.black);
            }
            _lut.Apply();
            
            File.WriteAllBytes("C:\\out.png", _lut.EncodeToPNG());
        }
        
        private Color _lutAt(int x, int y)
        {
            return new Color((x % s_LutSize) / (s_LutSize - 1f), y / (s_LutSize - 1f), Mathf.FloorToInt(x / (float)s_LutSize) * (1f / (s_LutSize - 1f)), 1f);
        }
    }
}