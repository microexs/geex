﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Geex.Shared._ShouldMigrateToLib.Abstractions
{
    /// <summary>
    /// A base type to use for creating smart enums with inner value of type <see cref="System.Int32"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type that is inheriting from this class.</typeparam>
    /// <remarks></remarks>
    public abstract class Enumeration<TEnum> :
        Enumeration<TEnum, int>
        where TEnum : Enumeration<TEnum, int>
    {
        protected Enumeration(string name, int value) :
            base(name, value)
        {
        }
    }

    /// <summary>
    /// A base type to use for creating smart enums.
    /// </summary>
    /// <typeparam name="TEnum">The type that is inheriting from this class.</typeparam>
    /// <typeparam name="TValue">The type of the inner value.</typeparam>
    /// <remarks></remarks>
    public abstract class Enumeration<TEnum, TValue> :
        IEquatable<Enumeration<TEnum, TValue>>,
        IComparable<Enumeration<TEnum, TValue>>
        where TEnum : Enumeration<TEnum, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
    {
        static readonly Lazy<Dictionary<string, TEnum>> _fromName =
            new Lazy<Dictionary<string, TEnum>>(() => GetAllOptions().ToDictionary(item => item.Name));

        static readonly Lazy<Dictionary<string, TEnum>> _fromNameIgnoreCase =
            new Lazy<Dictionary<string, TEnum>>(() => GetAllOptions().ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase));

        static readonly Lazy<Dictionary<TValue, TEnum>> _fromValue =
            new Lazy<Dictionary<TValue, TEnum>>(() =>
            {
                // multiple enums with same value are allowed but store only one per value
                var dictionary = new Dictionary<TValue, TEnum>();
                foreach (var item in GetAllOptions())
                {
                    if (!dictionary.ContainsKey(item._value))
                        dictionary.Add(item._value, item);
                }
                return dictionary;
            });

        private static IEnumerable<TEnum> GetAllOptions()
        {
            Type baseType = typeof(TEnum);
            IEnumerable<Type> enumTypes = Assembly.GetAssembly(baseType).GetTypes().Where(t => baseType.IsAssignableFrom(t));

            List<TEnum> options = new List<TEnum>();
            foreach (Type enumType in enumTypes)
            {
                List<TEnum> typeEnumOptions = enumType.GetFieldsOfType<TEnum>();
                options.AddRange(typeEnumOptions);
            }

            return options.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Gets a collection containing all the instances of <see cref="Enumeration{TEnum}"/>.
        /// </summary>
        /// <value>A <see cref="IReadOnlyCollection{TEnum}"/> containing all the instances of <see cref="Enumeration{TEnum}"/>.</value>
        /// <remarks>Retrieves all the instances of <see cref="Enumeration{TEnum}"/> referenced by public static read-only fields in the current class or its bases.</remarks>
        public static IReadOnlyCollection<TEnum> List =>
            _fromName.Value.Values
                .ToList()
                .AsReadOnly();

        private readonly string _name;
        private readonly TValue _value;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>A <see cref="String"/> that is the name of the <see cref="Enumeration{TEnum}"/>.</value>
        public string Name =>
            _name;

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>A <typeparamref name="TValue"/> that is the value of the <see cref="Enumeration{TEnum}"/>.</value>
        public TValue Value =>
            _value;

        protected Enumeration(string name, TValue value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _name = name;
            _value = value;
        }

        /// <summary>
        /// construct a enum with name of value.ToString()
        /// </summary>
        /// <param name="value"></param>
        protected Enumeration(TValue value)
        {
            _name = value.ToString() ?? throw new InvalidOperationException("simple enum value must have a equation of string.");
            _value = value;
        }

        /// <summary>
        /// Gets the type of the inner value.
        /// </summary>
        /// <value>A <see name="System.Type"/> that is the type of the value of the <see cref="Enumeration{TEnum}"/>.</value>
        public Type GetValueType() =>
            typeof(TValue);

        /// <summary>
        /// Gets the item associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the item to get.</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case during the comparison; otherwise, <c>false</c>.</param>
        /// <returns>
        /// The item associated with the specified name. 
        /// If the specified name is not found, throws a <see cref="KeyNotFoundException"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>.</exception> 
        /// <exception cref="SmartEnumNotFoundException"><paramref name="name"/> does not exist.</exception> 
        /// <seealso cref="Enumeration{TEnum}.TryFromName(string, out TEnum)"/>
        /// <seealso cref="Enumeration{TEnum}.TryFromName(string, bool, out TEnum)"/>
        public static TEnum FromName(string name, bool ignoreCase = false)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (ignoreCase)
                return FromName(_fromNameIgnoreCase.Value);
            else
                return FromName(_fromName.Value);

            TEnum FromName(Dictionary<string, TEnum> dictionary)
            {
                if (!dictionary.TryGetValue(name, out var result))
                {
                    throw new KeyNotFoundException(name);
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the item associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the item to get.</param>
        /// <param name="result">
        /// When this method returns, contains the item associated with the specified name, if the key is found; 
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Enumeration{TEnum}"/> contains an item with the specified name; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>.</exception> 
        /// <seealso cref="Enumeration{TEnum}.FromName(string, bool)"/>
        /// <seealso cref="Enumeration{TEnum}.TryFromName(string, bool, out TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFromName(string name, out TEnum result) =>
            TryFromName(name, false, out result);

        /// <summary>
        /// Gets the item associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the item to get.</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case during the comparison; otherwise, <c>false</c>.</param>
        /// <param name="result">
        /// When this method returns, contains the item associated with the specified name, if the name is found; 
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Enumeration{TEnum}"/> contains an item with the specified name; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>.</exception> 
        /// <seealso cref="Enumeration{TEnum}.FromName(string, bool)"/>
        /// <seealso cref="Enumeration{TEnum}.TryFromName(string, out TEnum)"/>
        public static bool TryFromName(string name, bool ignoreCase, out TEnum result)
        {
            if (String.IsNullOrEmpty(name))
            {
                result = default;
                return false;
            }

            if (ignoreCase)
                return _fromNameIgnoreCase.Value.TryGetValue(name, out result);
            else
                return _fromName.Value.TryGetValue(name, out result);
        }

        /// <summary>
        /// Gets an item associated with the specified value.
        /// </summary>
        /// <param name="value">The value of the item to get.</param>
        /// <returns>
        /// The first item found that is associated with the specified value.
        /// If the specified value is not found, throws a <see cref="KeyNotFoundException"/>.
        /// </returns>
        /// <exception cref="SmartEnumNotFoundException"><paramref name="value"/> does not exist.</exception> 
        /// <seealso cref="Enumeration{TEnum}.FromValue(TValue, TEnum)"/>
        /// <seealso cref="Enumeration{TEnum}.TryFromValue(TValue, out TEnum)"/>
        public static TEnum FromValue(TValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!_fromValue.Value.TryGetValue(value, out var result))
            {
                throw new KeyNotFoundException(value.ToString());
            }
            return result;
        }

        /// <summary>
        /// Gets an item associated with the specified value.
        /// </summary>
        /// <param name="value">The value of the item to get.</param>
        /// <param name="defaultValue">The value to return when item not found.</param>
        /// <returns>
        /// The first item found that is associated with the specified value.
        /// If the specified value is not found, returns <paramref name="defaultValue"/>.
        /// </returns>
        /// <seealso cref="Enumeration{TEnum}.FromValue(TValue)"/>
        /// <seealso cref="Enumeration{TEnum}.TryFromValue(TValue, out TEnum)"/>
        public static TEnum FromValue(TValue value, TEnum defaultValue)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!_fromValue.Value.TryGetValue(value, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        /// <summary>
        /// Gets an item associated with the specified value.
        /// </summary>
        /// <param name="value">The value of the item to get.</param>
        /// <param name="result">
        /// When this method returns, contains the item associated with the specified value, if the value is found; 
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Enumeration{TEnum}"/> contains an item with the specified name; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="Enumeration{TEnum}.FromValue(TValue)"/>
        /// <seealso cref="Enumeration{TEnum}.FromValue(TValue, TEnum)"/>
        public static bool TryFromValue(TValue value, out TEnum result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            return _fromValue.Value.TryGetValue(value, out result);
        }

        public override string ToString() =>
            _name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() =>
            _value.GetHashCode();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) =>
            (obj is Enumeration<TEnum, TValue> other) && Equals(other);

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Enumeration{TEnum}"/> value.
        /// </summary>
        /// <param name="other">An <see cref="Enumeration{TEnum}"/> value to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> has the same value as this instance; otherwise, <c>false</c>.</returns>
        public virtual bool Equals(Enumeration<TEnum, TValue> other)
        {
            // check if same instance
            if (Object.ReferenceEquals(this, other))
                return true;

            // it's not same instance so 
            // check if it's not null and is same value
            if (other is null)
                return false;

            return _value.Equals(other._value);
        }

        public static bool operator ==(Enumeration<TEnum, TValue> left, Enumeration<TEnum, TValue> right)
        {
            // Handle null on left side
            if (left is null)
                return right is null; // null == null = true

            // Equals handles null on right side
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Enumeration<TEnum, TValue> left, Enumeration<TEnum, TValue> right) =>
            !(left == right);

        /// <summary>
        /// Compares this instance to a specified <see cref="Enumeration{TEnum}"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An <see cref="Enumeration{TEnum}"/> value to compare to this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual int CompareTo(Enumeration<TEnum, TValue> other) =>
            _value.CompareTo(other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Enumeration<TEnum, TValue> left, Enumeration<TEnum, TValue> right) =>
            left.CompareTo(right) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Enumeration<TEnum, TValue> left, Enumeration<TEnum, TValue> right) =>
            left.CompareTo(right) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Enumeration<TEnum, TValue> left, Enumeration<TEnum, TValue> right) =>
            left.CompareTo(right) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Enumeration<TEnum, TValue> left, Enumeration<TEnum, TValue> right) =>
            left.CompareTo(right) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TValue(Enumeration<TEnum, TValue> enumeration) =>
            enumeration._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Enumeration<TEnum, TValue>(TValue value) =>
            FromValue(value);
    }

    public static class EnumerationExtensions
    {
        public static IEnumerable<TResult> Cast<TEnum, TResult>(this IEnumerable<TEnum> source) where TEnum : Enumeration<TEnum, TResult> where TResult : IEquatable<TResult>, IComparable<TResult>
        {
            if (source is IEnumerable<TResult> results)
                return results;
            if (source == null)
                throw new ArgumentNullException("source");
            return source.Select(x => (TResult)x);
        }
    }
}
