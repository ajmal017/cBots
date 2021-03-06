﻿#region Licence
//The MIT License (MIT)
//Copyright (c) 2014 abdallah HACID, https://www.facebook.com/ab.hacid

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software
//and associated documentation files (the "Software"), to deal in the Software without restriction,
//including without limitation the rights to use, copy, modify, merge, publish, distribute,
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
//is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or
//substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
//BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Project Hosting for Open Source Software on Codeplex : https://calgobots.codeplex.com/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cAlgo.Lib
{
	/// <summary>
	/// Extensions methods of boolean
	/// </summary>
	public static class BooleanExtensions
	{
		/// <summary>
		/// Converts true to 1 and false to 0
		/// </summary>
		/// <param name="b">Boolean to test</param>
		/// <returns>returns 1 if b = true 0 otherwise</returns>
		public static int toInt(this Boolean b)
		{
			return b ? 1 : 0;
		}

		/// <summary>
		/// Convert true to 1 and false to -1
		/// </summary>
		/// <param name="b">Boolean to test</param>
		/// <returns>returns 1 if b=true, -1 otherwise</returns>
		public static int factor(this Boolean b)
		{
			return b ? 1 : -1;
		}
	}
}
