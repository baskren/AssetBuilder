using System;
using System.Drawing;

namespace AndroidVector
{
	public sealed class Matrix : MarshalByRefObject, IDisposable
	{

		static float[] _identity = new float[] { 1, 0, 0, 1, 0, 0 };
		MatrixTypes _type = MatrixTypes.Identity;

		float[] _elements = new float[] { 1, 0, 0, 1, 0, 0 };
		public float[] Elements => _elements;

		float A
		{
			get => _elements[0];
			set => _elements[0] = value;
		}

		float B
		{
			get => _elements[1];
			set => _elements[1] = value;
		}

		float C
		{
			get => _elements[2];
			set => _elements[2] = value;
		}

		float D
		{
			get => _elements[3];
			set => _elements[3] = value;
		}

		float E
		{
			get => _elements[4];
			set => _elements[4] = value;
		}

		float F
		{
			get => _elements[5];
			set => _elements[5] = value;
		}


		public bool IsIdentity =>
			Elements[0] == 1 && Elements[1] == 0 &&
			Elements[2] == 0 && Elements[3] == 1 &&
			Elements[4] == 0 && Elements[5] == 0;

		public bool IsInvertible
		{
			get
			{
				if (B == 0 && C == 0)
					return A != 0 && D != 0;
				return Math.Abs(Determinant()) >= 1e-5;
			}
		}

		public float OffsetX => Elements[4];

		public float OffsetY => Elements[5];

		public float ScaleX => (float)Math.Sqrt(A * A + C * C);

		public float ScaleY => (float)Math.Sqrt(B * B + D * D);

		public float RotationRadians => (float)Math.Atan2(B, A);

		public float RotationDegrees => (float)(180 * RotationRadians / Math.PI);

		public Matrix()
		{
		}

		public Matrix(Rectangle rect, Point[] pt)
		{
			if (pt is null)
				throw new ArgumentNullException(nameof(pt));
			if (pt.Length != 3)
				throw new ArgumentException(nameof(pt));

			A = (pt[1].X - pt[0].X) / rect.Width;
			C = (pt[2].X - pt[0].X) / rect.Height;
			E = pt[0].X - A * rect.X - C * rect.Y;
			B = (pt[1].Y - pt[0].Y) / rect.Width;
			D = (pt[2].Y - pt[0].Y) / rect.Height;
			F = pt[0].Y - B * rect.X - D * rect.Y;
		}

		public Matrix(RectangleF rect, PointF[] pt)
		{
			if (pt is null)
				throw new ArgumentNullException(nameof(pt));
			if (pt.Length != 3)
				throw new ArgumentException(nameof(pt));

			A = (pt[1].X - pt[0].X) / rect.Width;
			C = (pt[2].X - pt[0].X) / rect.Height;
			E = pt[0].X - A * rect.X - C * rect.Y;
			B = (pt[1].Y - pt[0].Y) / rect.Width;
			D = (pt[2].Y - pt[0].Y) / rect.Height;
			F = pt[0].Y - B * rect.X - D * rect.Y;
		}

		public Matrix(float a, float b, float c, float d, float e, float f)
		{
			A = a;
			B = b;
			C = c;
			D = d;
			E = e;
			F = f;
			_type = MatrixTypes.Unknown;
			DeriveType();
		}

		public static Matrix CreateTranslate(float dx, float dy)
			=> new Matrix(1, 0, 0, 1, dx, dy);

        public static Matrix CreateRotateDegrees(float degrees, PointF center = default)
            => CreateRotateRadians((float)Math.PI * degrees / 180f, center);

		public static Matrix CreateRotateRadians(float radians, PointF center = default)
		{
			var px = center.X;
			var py = center.Y;
			var cos = (float)Math.Cos(radians);
			var sin = (float)Math.Sin(radians);
			return new Matrix(cos, sin, -sin, cos, px - px * cos + py * sin, py - py * cos - px * sin);
		}

		public static Matrix CreateScale(float scale)
			=> new Matrix(scale, 0, 0, scale, 0, 0);

		public static Matrix CreateScale(float scaleX, float scaleY)
		    => new Matrix(scaleX, 0, 0, scaleY, 0, 0);

        public static Matrix CreateSkewDegrees(float degreesX, float degreesY)
            => CreateSkewRadians((float)(Math.PI * degreesX / 180f), (float)(Math.PI* degreesY / 180f));

		public static Matrix CreateSkewRadians(float radiansX, float radiansY)
		{
			if (radiansX % Math.PI / 2 == 0 && (radiansX / (Math.PI /2))%2 > 0)
                System.Diagnostics.Debug.WriteLine("Matrix");
			if (radiansY % Math.PI / 2 == 0 && (radiansY / (Math.PI / 2)) % 2 > 0)
				System.Diagnostics.Debug.WriteLine("Matrix");

			return CreateShear((float)Math.Tan(radiansX), (float)Math.Tan(radiansY));
		}

		public static Matrix CreateShear(float shearX, float shearY)
			=> new Matrix(1, shearY, shearX, 1, 0, 0);



		public Matrix Clone()
			=> new Matrix(Elements[0], Elements[1], Elements[2], Elements[3], Elements[4], Elements[5]);

		public void Dispose()
		{
		}

		public override bool Equals(object obj)
		{
			if (obj is Matrix other)
			{
				return
					Elements[0] == other.Elements[0] &&
					Elements[1] == other.Elements[1] &&
					Elements[2] == other.Elements[2] &&
					Elements[3] == other.Elements[3] &&
					Elements[4] == other.Elements[4] &&
					Elements[5] == other.Elements[5];
			}
			return false;
		}

		public void Invert()
		{
			if (!IsInvertible)
				return;
			if (B == 0 && C == 0)
			{
				E = -E / A;
				F = -F / D;
				A = 1 / A;
				D = 1 / D;
			}
			else
			{
				var det = Determinant();
				var copy = Clone();
				A = copy.D / det;
				B = -copy.B / det;
				C = -copy.C / det;
				D = copy.A / det;
				E = (copy.C * copy.F - copy.D * copy.E) / det;
				F = (copy.A * copy.F - copy.B * copy.E) / det;
			}
		}

		public void Multiply(Matrix matrix)
			=> Multiply(matrix, MatrixOrder.Prepend);

		public void Multiply(Matrix matrix, MatrixOrder order)
		{
			if (matrix is null)
				return;

			if (order == MatrixOrder.Append)
				_elements = Mul(this._elements, matrix._elements);
			else
				_elements = Mul(matrix._elements, this._elements);
		}

		static float[] Mul(float[] a1, float[] a2)
		{
			var r = new float[6];

			r[0] = a1[0] * a2[0] + a1[1] * a2[2];
			r[1] = a1[0] * a2[1] + a1[1] * a2[3];
			r[2] = a1[2] * a2[0] + a1[3] * a2[2];
			r[3] = a1[2] * a2[1] + a1[3] + a2[3];
			r[4] = a1[4] * a2[0] + a1[5] * a2[2] + a2[4];
			r[5] = a1[4] * a2[1] + a1[5] * a2[3] + a2[5];
			return r;
		}

		public void Reset()
		{
			_elements = new float[] { 1, 0, 0, 1, 0, 0 };
		}

		public void Rotate(float angle)
			=> Rotate(angle, MatrixOrder.Prepend);

		public void Rotate(float angle, MatrixOrder order)
		{
			float cos_theta, sin_theta;
			float[] rotate = new float[6];

			angle *= (float)(Math.PI / 180.0);
			cos_theta = (float)Math.Cos(angle);
			sin_theta = (float)Math.Sin(angle);
			rotate[0] = cos_theta;
			rotate[1] = sin_theta;
			rotate[2] = -sin_theta;
			rotate[3] = cos_theta;
			rotate[4] = 0.0f;
			rotate[5] = 0.0f;

			if (order == MatrixOrder.Append)
				_elements = Mul(_elements, rotate);
			else
				_elements = Mul(rotate, _elements);
		}

		public void RotateAt(float angle, PointF point)
			=> RotateAt(angle, point, MatrixOrder.Prepend);

		public void RotateAt(float angle, PointF point, MatrixOrder order)
		{
			angle *= (float)(Math.PI / 180.0);  // degrees to radians
			var cos = (float)Math.Cos(angle);
			var sin = (float)Math.Sin(angle);
			var e4 = -point.X * cos + point.Y * sin + point.X;
			var e5 = -point.X * sin - point.Y * cos + point.Y;
			var m = new float[] { A, B, C, D, E, F };

			if (order == MatrixOrder.Prepend)
			{
				A = cos * m[0] + sin * m[2];
				B = cos * m[1] + sin * m[3];
				C = -sin * m[0] + cos * m[2];
				D = -sin * m[1] + cos * m[3];
				E = e4 * m[0] + e5 * m[2] + m[4];
				F = e4 * m[1] + e5 * m[3] + m[5];
			}
			else
			{
				A = m[0] * cos + m[1] * -sin;
				B = m[0] * sin + m[1] * cos;
				C = m[2] * cos + m[3] * -sin;
				D = m[2] * sin + m[3] * cos;
				E = m[4] * cos + m[5] * -sin + e4;
				F = m[4] * sin + m[5] * cos + e5;
			}
		}

		public void Scale(float scale)
			=> Scale(scale, scale, MatrixOrder.Prepend);

		public void Scale(float scaleX, float scaleY)
			=> Scale(scaleX, scaleY, MatrixOrder.Prepend);

		public void Scale(float scaleX, float scaleY, MatrixOrder order)
		{
			var scale = new float[] { scaleX, 0, 0, scaleY, 0, 0 };

			if (order == MatrixOrder.Append)
				_elements = Mul(_elements, scale);
			else
				_elements = Mul(scale, _elements);
		}

		public void Shear(float shearX, float shearY)
			=> Shear(shearX, shearY, MatrixOrder.Prepend);

		public void Shear(float shearX, float shearY, MatrixOrder order)
		{
			var shear = new float[] { 1, shearY, shearX, 1, 0, 0 };

			if (order == MatrixOrder.Append)
				_elements = Mul(_elements, shear);
			else
				_elements = Mul(shear, _elements);
		}

		public PointF TransformPoint(PointF pt)
		{
			float x, y;
			x = pt.X * A + pt.Y * C + E;
			y = pt.X * B + pt.Y * D + F;
			return new PointF(x, y);
		}


		public void Translate(float offsetX, float offsetY)
		{
			Translate(offsetX, offsetY, MatrixOrder.Prepend);
		}

		public void Translate(float offsetX, float offsetY, MatrixOrder order)
		{
			var trans = new float[] { 1, 0, 0, 1, offsetX, offsetY };

			if (order == MatrixOrder.Append)
				_elements = Mul(_elements, trans);
			else
				_elements = Mul(trans, _elements);
		}


		void DeriveType()
		{
			_type = 0;
			if (!(C == 0 && B == 0))
			{
				_type = MatrixTypes.Unknown;
				return;
			}
			if (!(A == 1 && D == 1))
				_type = MatrixTypes.Scaling;
			if (!(E == 0 && F == 0))
				_type |= MatrixTypes.Translation;
			if (0 == (_type & (MatrixTypes.Translation | MatrixTypes.Scaling)))
				_type = MatrixTypes.Identity;
		}

		float Determinant()
		{
			return (float)(A * (double)D - B * (double)C);
		}

		public override int GetHashCode()
		{
			int hashCode = -1180554807;
			hashCode = hashCode * -1521134295 + A.GetHashCode();
			hashCode = hashCode * -1521134295 + B.GetHashCode();
			hashCode = hashCode * -1521134295 + D.GetHashCode();
			hashCode = hashCode * -1521134295 + E.GetHashCode();
			hashCode = hashCode * -1521134295 + F.GetHashCode();
			return hashCode;
		}
	}
}