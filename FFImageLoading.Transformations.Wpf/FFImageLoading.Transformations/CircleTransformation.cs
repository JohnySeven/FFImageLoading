using FFImageLoading.Work;

namespace FFImageLoading.Transformations
{
	public class CircleTransformation : TransformationBase
	{
		public double BorderSize
		{
			get;
			set;
		}

		public string BorderHexColor
		{
			get;
			set;
		}

		public override string Key => $"CircleTransformation,borderSize={BorderSize},borderHexColor={BorderHexColor}";

		public CircleTransformation()
			: this(0.0, null)
		{
		}

		public CircleTransformation(double borderSize, string borderHexColor)
		{
			BorderSize = borderSize;
			BorderHexColor = borderHexColor;
		}

		protected override BitmapHolder Transform(BitmapHolder bitmapSource, string path, ImageSource source, bool isPlaceholder, string key)
		{
			return RoundedTransformation.ToRounded(bitmapSource, 0, 1.0, 1.0, BorderSize, BorderHexColor);
		}
	}
}
