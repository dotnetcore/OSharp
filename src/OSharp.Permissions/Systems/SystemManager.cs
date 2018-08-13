﻿// -----------------------------------------------------------------------
//  <copyright file="SystemManager.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2018 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2018-06-25 21:00</last-date>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

using OSharp.Caching;
using OSharp.Core.Systems;
using OSharp.Data;
using OSharp.Entity;


namespace OSharp.Systems
{
    /// <summary>
    /// 系统管理器
    /// </summary>
    public class SystemManager : IKeyValueStore
    {
        private readonly IRepository<KeyValue, Guid> _keyValueRepository;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// 初始化一个<see cref="SystemManager"/>类型的新实例
        /// </summary>
        public SystemManager(IRepository<KeyValue, Guid> keyValueRepository,
            IDistributedCache cache)
        {
            _keyValueRepository = keyValueRepository;
            _cache = cache;
        }

        #region Implementation of IKeyValueCoupleStore

        private const string AllKeyValuesKey = "All_KeyValue_Key";

        /// <summary>
        /// 获取 键值对数据查询数据集
        /// </summary>
        public IQueryable<KeyValue> KeyValues
        {
            get { return _keyValueRepository.Query(); }
        }

        /// <summary>
        /// 获取指定键名的数据项
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>数据项</returns>
        public KeyValue GetKeyValue(string key)
        {
            const int seconds = 60 * 1000;
            KeyValue[] pairs = _cache.Get(AllKeyValuesKey, () => _keyValueRepository.Query().ToArray(), seconds);
            return pairs.FirstOrDefault(m => m.Key == key);
        }

        /// <summary>
        /// 检查键值对信息信息是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的键值对信息编号</param>
        /// <returns>键值对信息是否存在</returns>
        public Task<bool> CheckKeyValueExists(Expression<Func<KeyValue, bool>> predicate, Guid id = default(Guid))
        {
            return _keyValueRepository.CheckExistsAsync(predicate, id);
        }

        /// <summary>
        /// 添加或更新键值对信息信息
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>业务操作结果</returns>
        public Task<OperationResult> CreateOrUpdateKeyValue(string key, object value)
        {
            KeyValue pair = new KeyValue(key, value);
            return CreateOrUpdateKeyValues(pair);
        }

        /// <summary>
        /// 添加或更新键值对信息信息
        /// </summary>
        /// <param name="dtos">要添加的键值对信息DTO信息</param>
        /// <returns>业务操作结果</returns>
        public async Task<OperationResult> CreateOrUpdateKeyValues(params KeyValue[] dtos)
        {
            Check.NotNull(dtos, nameof(dtos));
            foreach (KeyValue dto in dtos)
            {
                KeyValue pair = _keyValueRepository.TrackQuery().FirstOrDefault(m => m.Key == dto.Key);
                if (pair == null)
                {
                    pair = dto;
                    await _keyValueRepository.InsertAsync(pair);
                }
                else
                {
                    pair.Value = dto.Value;
                    await _keyValueRepository.UpdateAsync(pair);
                }
            }
            await _cache.RemoveAsync(AllKeyValuesKey);
            return OperationResult.Success;
        }

        /// <summary>
        /// 删除键值对信息信息
        /// </summary>
        /// <param name="ids">要删除的键值对信息编号</param>
        /// <returns>业务操作结果</returns>
        public async Task<OperationResult> DeleteKeyValues(params Guid[] ids)
        {
            OperationResult result = await _keyValueRepository.DeleteAsync(ids);
            if (result.Successed)
            {
                await _cache.RemoveAsync(AllKeyValuesKey);
            }
            return result;
        }

        /// <summary>
        /// 删除以根键路径为起始的所有字典项，如输入“System.User.”，所有键以此开头的项都会被删除
        /// </summary>
        /// <param name="rootKey">根键路径</param>
        /// <returns>业务操作结果</returns>
        public async Task<OperationResult> DeleteKeyValues(string rootKey)
        {
            Guid[] ids = _keyValueRepository.Query(m => m.Key.StartsWith(rootKey)).Select(m => m.Id).ToArray();
            return await DeleteKeyValues(ids);
        }

        #endregion
    }
}