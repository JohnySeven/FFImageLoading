using System;

namespace FFImageLoading.Transformations
{
	[Flags]
	public enum CornerTransformType
	{
		TopLeftCut = 0x1,
		TopRightCut = 0x2,
		BottomLeftCut = 0x4,
		BottomRightCut = 0x8,
		TopLeftRounded = 0x10,
		TopRightRounded = 0x20,
		BottomLeftRounded = 0x40,
		BottomRightRounded = 0x80,
		AllCut = 0xF,
		LeftCut = 0x5,
		RightCut = 0xA,
		TopCut = 0x3,
		BottomCut = 0xC,
		AllRounded = 0xF0,
		LeftRounded = 0x50,
		RightRounded = 0xA0,
		TopRounded = 0x30,
		BottomRounded = 0xC0
	}
}
