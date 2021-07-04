using System.Globalization;

public static class Locale
{
    private static NumberFormatInfo mFormat;
    public static NumberFormatInfo Format
    {
        get
        {
            if (mFormat == null)
            {
                mFormat = new NumberFormatInfo();
                mFormat.NumberDecimalSeparator = ".";
                mFormat.NumberGroupSeparator = " ";
                mFormat.NumberGroupSizes = new int[1] { 3 };
                mFormat.NumberDecimalDigits = 0;
            }
            return mFormat;
        }
    }
}
