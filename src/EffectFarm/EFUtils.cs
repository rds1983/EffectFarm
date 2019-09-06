using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Reflection;

namespace EffectFarm
{
	public static class EFUtils
	{
		private static EFPlatform? _currentPlatform = null;

		public static EFPlatform CurrentPlatform
		{
			get
			{
				if (_currentPlatform != null)
				{
					return _currentPlatform.Value;
				}

#if FNA
				_currentPlatform = EFPlatform.FNA;
#else
				var isOpenGL = (from f in typeof(GraphicsDevice).GetFields(BindingFlags.NonPublic |
						 BindingFlags.Instance)
								where f.Name == "glFramebuffer"
								select f).FirstOrDefault() != null;
				_currentPlatform = isOpenGL ? EFPlatform.MonoGameOpenGL : 
					EFPlatform.MonoGameDirectX;
#endif

				return _currentPlatform.Value;
			}
		}
	}
}
