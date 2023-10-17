#define DISABLE_MULTI_MAP

using System;
namespace WHMapper
{
	public static class FeatureFlag
	{
        public static bool DISABLE_MULTI_MAP()
        {
            bool value = false;
            #if (DISABLE_MULTI_MAP)
                value = true;
            #endif
            return value;
        }
    }
}

