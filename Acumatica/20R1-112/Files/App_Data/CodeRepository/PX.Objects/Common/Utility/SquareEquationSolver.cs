using System;
using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// A square equation solver.
	/// </summary>
	public static class SquareEquationSolver
	{
		/// <summary>
		/// Calculate square root for <see cref="decimal"/> number <paramref name="a"/>. The result of the calculations will differ from an actual value of the root on less than epslion.
		/// More details about algorithm are here: https://stackoverflow.com/questions/4124189/performing-math-operations-on-decimal-datatype-in-c
		/// and here: https://doc.lagout.org/security/Hackers%20Delight.pdf#%5B%7B%22num%22%3A1017%2C%22gen%22%3A0%7D%2C%7B%22name%22%3A%22XYZ%22%7D%2C5%2C738%2Cnull%5D
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="a"/> or <paramref name="epsilon"/> is less than zero.</exception>
		/// <param name="a">The number, from which we need to calculate the square root.</param>
		/// <param name="epsilon">(Optional) The accuracy of calculation of the root from our number.</param>
		/// <returns/>
		public static decimal Sqrt(this decimal a, decimal epsilon = 0.0M)
		{
			if (a < 0m)
				throw new ArgumentOutOfRangeException("Cannot calculate square root from a negative number");
			else if (epsilon < 0m)
				throw new ArgumentOutOfRangeException($"{nameof(epsilon)} can't be a negative number");

			decimal previous;
			decimal current = (decimal)Math.Sqrt((double)a);	//statring point

			do
			{
				previous = current;

				if (previous == 0.0M) 
					return 0;

				// Iterative search for solution by using Newton's method for searching roots for equation F(x) = 0. 
				// We need to calculate sqrt(a) so the equation looks like: x^2 - a = 0.  So F(x) = x^2 - a
				// The Newton's method is an iterative search: X n+1 = X n - F(X n) / F'(x n)  
				// If we substitute F(x) = x^2 - a we will get X n+1 = (X n + (a / X n)) / 2
				current = (previous + (a / previous)) / 2;		
			}
			while (Math.Abs(previous - current) > epsilon);

			return current;
		}

		/// <summary>
		/// Solve quadratic equation a*X^2 + b*x + c = 0. Returns <c>null</c> if there is no solution for the equation. 
		/// All calculations done with <see cref="double"/>.
		/// </summary>
		/// <param name="a">Coefficient a.</param>
		/// <param name="b">Coefficient b.</param>
		/// <param name="c">Coefficient c.</param>
		/// <returns/>
		public static (double X1, double X2)? SolveQuadraticEquation(double a, double b, double c)
		{	
			double determinant = (b * b) - (4 * a * c);
			double x1, x2;

			if (determinant > 0)
			{
				double sqrtD = Math.Sqrt(determinant);
				x1 = (-b - sqrtD) / (2 * a);
				x2 = (-b + sqrtD) / (2 * a);
				return (x1, x2);
			}
			else if (determinant == 0)
			{
				x1 = x2 = (-b) / (2 * a);
				return (x1, x2);
			}
			else
				return null;	
		}

		/// <summary>
		/// Solve quadratic equation a*X^2 + b*x + c = 0. Returns <c>null</c> if there is no solution for the equation. 
		/// All calculations done with <see cref="decimal"/>.
		/// </summary>
		/// <param name="a">Coefficient a.</param>
		/// <param name="b">Coefficient b.</param>
		/// <param name="c">Coefficient c.</param>
		/// <returns/>
		public static (decimal X1, decimal X2)? SolveQuadraticEquation(decimal a, decimal b, decimal c)
		{
			decimal determinant = (b * b) - (4 * a * c);
			decimal x1, x2;

			if (determinant > 0)
			{
				decimal sqrtD = Sqrt(determinant);
				x1 = (-b - sqrtD) / (2 * a);
				x2 = (-b + sqrtD) / (2 * a);
				return (x1, x2);
			}
			else if (determinant == 0)
			{
				x1 = x2 = (-b) / (2 * a);
				return (x1, x2);
			}
			else
				return null;
		}
	}
}
