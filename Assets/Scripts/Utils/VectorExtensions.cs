using System;
using UnityEngine;

namespace Bas.Pennings.DevTools
{
    /// <summary>
    /// Provides extension methods for <see cref="Vector2"/> 
    /// and <see cref="Vector3"/> to enhance their functionality.
    /// </summary>
    public static class VectorExtensions
    {

        /// <summary>
        /// Deconstructs a <see cref="Vector2"/> into its individual components.
        /// </summary>
        /// <param name="v">The <see cref="Vector2"/> to deconstruct.</param>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        public static void Deconstruct(this Vector2 v, out float x, out float y)
        {
            x = v.x;
            y = v.y;
        }

        /// <summary>
        /// Deconstructs a <see cref="Vector3"/> into its individual components.
        /// </summary>
        /// <param name="v">The <see cref="Vector3"/> to deconstruct.</param>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        /// <summary>
        /// Rounds each component of a <see cref="Vector2"/> to the nearest integer.
        /// </summary>
        /// <param name="v">The <see cref="Vector2"/> to round.</param>
        /// <returns>A new <see cref="Vector2"/> with rounded components.</returns>
        public static Vector2 Round(this Vector2 v)
            => v.ModifyWithFunction((val, _) => Mathf.Round(val));

        /// <summary>
        /// Rounds each component of a <see cref="Vector3"/> to the nearest integer.
        /// </summary>
        /// <param name="v">The <see cref="Vector3"/> to round.</param>
        /// <returns>A new <see cref="Vector3"/> with rounded components.</returns>
        public static Vector3 Round(this Vector3 v)
            => v.ModifyWithFunction((val, _) => Mathf.Round(val));

        /// <summary>
        /// Clamps each component of a <see cref="Vector2"/> between a minimum and maximum value.
        /// </summary>
        /// <param name="v">The <see cref="Vector2"/> to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A new <see cref="Vector2"/> with clamped components.</returns>
        public static Vector2 Clamp(this Vector2 v, float min, float max)
            => v.ModifyWithFunction((val, args) => Mathf.Clamp(val, args[0], args[1]), min, max);

        /// <summary>
        /// Clamps each component of a <see cref="Vector3"/> between a minimum and maximum value.
        /// </summary>
        /// <param name="v">The <see cref="Vector3"/> to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A new <see cref="Vector3"/> with clamped components.</returns>
        public static Vector3 Clamp(this Vector3 v, float min, float max)
            => v.ModifyWithFunction((val, args) => Mathf.Clamp(val, args[0], args[1]), min, max);

        /// <summary>
        /// Applies a mathematical function to each component of a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="v">The <see cref="Vector2"/> to modify.</param>
        /// <param name="method">A function that takes a <see cref="float"/> value and an optional <see cref="float"/> array of parameters.</param>
        /// <param name="args">Additional <see cref="float"/> parameters for the function.</param>
        /// <returns>A new <see cref="Vector2"/> with the function applied.</returns>
        /// <example>
        /// <code>
        /// Vector2 v = new Vector2(-3.5f, 7.8f);
        /// Vector2 absValue = v.ModifyWithFunction(Mathf.Abs); // Result: (3.5, 7.8)
        /// Vector2 clampValue = v.ModifyWithFunction((val, args) => Mathf.Clamp(val, args[0], args[1]), 2, 5); // Result: (2, 5)
        /// </code>
        /// </example>
        private static Vector2 ModifyWithFunction(this Vector2 v, Func<float, float[], float> method, params float[] args)
            => new(method(v.x, args), method(v.y, args));

        /// <summary>
        /// Applies a mathematical function to each component of a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="v">The <see cref="Vector3"/> to modify.</param>
        /// <param name="method">A function that takes a <see cref="float"/> value and an optional <see cref="float"/> array of parameters.</param>
        /// <param name="args">Additional <see cref="float"/> parameters for the function.</param>
        /// <returns>A new <see cref="Vector3"/> with the function applied.</returns>
        /// <example>
        /// <code>
        /// Vector2 v = new Vector3(-3.5f, 7.8f, 4.6f);
        /// Vector2 absValue = v.ModifyWithFunction(Mathf.Abs); // Result: (3.5, 7.8, 4.6)
        /// Vector2 clampValue = v.ModifyWithFunction((val, args) => Mathf.Clamp(val, args[0], args[1]), 2, 5); // Result: (2, 5, 4.6)
        /// </code>
        /// </example>
        private static Vector3 ModifyWithFunction(this Vector3 v, Func<float, float[], float> method, params float[] args)
            => new(method(v.x, args), method(v.y, args), method(v.z, args));
    }
}
