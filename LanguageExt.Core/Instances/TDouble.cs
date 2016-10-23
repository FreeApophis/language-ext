﻿using System;
using LanguageExt.TypeClasses;

namespace LanguageExt.Instances
{
    public struct TDouble : Ord<double>, Floating<double>
    {
        public bool Equals(double x, double y) => x == y;

        /// <summary>
        /// Compare two values
        /// </summary>
        /// <param name="x">Left hand side of the compare operation</param>
        /// <param name="y">Right hand side of the compare operation</param>
        /// <returns>
        /// if x greater than y : 1
        /// 
        /// if x less than y    : -1
        /// 
        /// if x equals y       : 0
        /// </returns>
        public int Compare(double x, double y) =>
            x.CompareTo(y);

        public double Plus(double x, double y) => x + y;
        public double Subtract(double x, double y) => x - y;
        public double Product(double x, double y) => x * y;

        /// <summary>
        /// Divide two numbers
        /// </summary>
        /// <param name="x">left hand side of the division operation</param>
        /// <param name="y">right hand side of the division operation</param>
        /// <returns>x / y</returns>
        public double Divide(double x, double y) =>
            x / y;

        public double Abs(double x) => Math.Abs(x);
        public double Signum(double x) => Math.Sign(x);
        public double FromInteger(int x) => (double)x;
        public double FromDecimal(decimal x) => (double)x;
        public double FromFloat(float x) => (double)x;
        public double FromDouble(double x) => (double)x;
        public double Div(double x, double y) => x / y;
        public double FromRational(Ratio<int> x) => x.Numerator / x.Denominator;
        public double Pi() => Math.PI;
        public double Exp(double x) => Math.Exp(x);
        public double Sqrt(double x) => Math.Sqrt(x);
        public double Log(double x) => Math.Log(x);
        public double Pow(double x, double y) => Math.Pow(x, y);
        public double LogBase(double b, double x) => Math.Log(x, b);
        public double Sin(double x) => Math.Sin(x);
        public double Cos(double x) => Math.Cos(x);
        public double Tan(double x) => Math.Tan(x);
        public double Asin(double x) => Math.Asin(x);
        public double Acos(double x) => Math.Acos(x);
        public double Atan(double x) => Math.Atan(x);
        public double Sinh(double x) => Math.Sinh(x);
        public double Cosh(double x) => Math.Cosh(x);
        public double Tanh(double x) => Math.Tanh(x);
        public double Asinh(double x) => Math.Log(x + Math.Sqrt((x * x) + 1.0));
        public double Acosh(double x) => Math.Log(x + Math.Sqrt((x * x) - 1.0));
        public double Atanh(double x) => 0.5 * Math.Log((1.0 + x) / (1.0 - x));
        public double Append(double x, double y) => x + y;
    }
}