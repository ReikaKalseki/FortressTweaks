using System;

namespace FortressTweaks
{
	public static class Util {    
	    public static double py3d(double rawX, double rawY, double rawZ, double rawX2, double rawY2, double rawZ2) {
	    	double dx = rawX2-rawX;
	    	double dy = rawY2-rawY;
	    	double dz = rawZ2-rawZ;
	    	return Math.Sqrt(dx*dx+dy*dy+dz*dz);
	    }
	}
}
