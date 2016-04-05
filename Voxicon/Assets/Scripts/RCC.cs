using UnityEngine;
using System;

// RCC8 relations
// grossly underspecified for now
namespace RCC
{
	public static class RCC8
	{
		// disconnected
		public static bool DC(Bounds x, Bounds y) {
			bool dc = false;

			if (!EC (x, y) && !PO (x, y) && !EQ (x, y) &&
			    !TPP (x, y) && !NTPP (x, y) && !TPPi (x, y) && !NTPPi (x, y)) {
				dc = true;
			}

			return dc;
		}

		// externally connected
		public static bool EC(Bounds x, Bounds y) {
			bool ec = false;
			
			// if y and z dimensions overlap
			if (Mathf.Abs(x.center.y - y.center.y) * 2 < (x.size.y + y.size.y) &&
				(Mathf.Abs(x.center.z - y.center.z) * 2 < (x.size.z + y.size.z))){
				if ((Mathf.Abs(x.min.x-y.max.x) < 0.002) ||	// if touching on y
					(Mathf.Abs(x.max.x-y.min.x) < 0.002)) {
					ec = true;
				}
			}
			// if x and z dimensions overlap
			if (Mathf.Abs(x.center.x - y.center.x) * 2 < (x.size.x + y.size.x) &&
				(Mathf.Abs(x.center.z - y.center.z) * 2 < (x.size.z + y.size.z))){
				if ((Mathf.Abs(x.min.y-y.max.y) < 0.002) ||	// if touching on y
					(Mathf.Abs(x.max.y-y.min.y) < 0.002)) {
					ec = true;
				}
			}
			// if x and y dimensions overlap
			if (Mathf.Abs (x.center.x - y.center.x) * 2 < (x.size.x + y.size.x) &&
			    (Mathf.Abs (x.center.y - y.center.y) * 2 < (x.size.y + y.size.y))) {
				if ((Mathf.Abs (x.min.z - y.max.z) < 0.002) ||	// if touching on z
				    (Mathf.Abs (x.max.z - y.min.z) < 0.002)) {
					ec = true;
				}
			}

			return ec;
		}

		public static bool PO(Bounds x, Bounds y) {
			bool po = false;

			if (x.Intersects (y)) {
				po = true;
			}

			return po;
		}

		public static bool EQ(Bounds x, Bounds y) {
			bool eq = false;

			if (((x.min-y.min).magnitude < 0.002) && ((x.max-y.max).magnitude < 0.002)) {
				eq = true;
			}

			return eq;
		}

		public static bool TPP(Bounds x, Bounds y) {
			bool tpp = false;

			if (y.Contains (x.min) && y.Contains (x.max)) {
				if ((Mathf.Abs(x.min.x-y.min.x) < 0.002) ||
					(Mathf.Abs(x.max.x-y.max.x) < 0.002) ||
					(Mathf.Abs(x.min.y-y.min.y) < 0.002) ||
					(Mathf.Abs(x.max.y-y.max.y) < 0.002) ||
					(Mathf.Abs(x.min.z-y.min.z) < 0.002) ||
					(Mathf.Abs(x.max.z-y.max.z) < 0.002))
				tpp = true;
			}

			return tpp;
		}

		public static bool NTPP(Bounds x, Bounds y) {
			bool ntpp = false;

			if (!TPP (x, y)) {
				if (y.Contains (x.min) && y.Contains (x.max)) {
					ntpp = true;
				}
			}

			return ntpp;
		}

		public static bool TPPi(Bounds x, Bounds y) {
			bool tppi = false;

			if (TPP (y, x)) {
				tppi = true;
			}

			return tppi;
		}

		public static bool NTPPi(Bounds x, Bounds y) {
			bool ntppi = false;

			if (NTPP (y, x)) {
				ntppi = true;
			}

			return ntppi;
		}

		public static bool ProperPart(Bounds x, Bounds y) {
			bool properpart = false;

			if (TPP (x, y) || NTPP (x, y)) {
				properpart = true;
			}

			return properpart;
		}
	}
}

