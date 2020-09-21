using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyAbp.Abp.DataDictionary.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EasyAbp.Abp.DataDictionary
{
    public class DataDictionaryAppService : ApplicationService, IDataDictionaryAppService
    {
        private readonly IDataDictionaryRepository _dataDictionaryRepository;
        private readonly IDataDictionaryManager _dataDictionaryManager;

        public DataDictionaryAppService(IDataDictionaryRepository dataDictionaryRepository,
            IDataDictionaryManager dataDictionaryManager)
        {
            _dataDictionaryRepository = dataDictionaryRepository;
            _dataDictionaryManager = dataDictionaryManager;
        }

        public async Task<DataDictionaryDto> CreateAsync(DataDictionaryCreateDto input)
        {
            var newDict = new DataDictionary(GuidGenerator.Create(),
                CurrentTenant.Id,
                input.Code,
                input.DisplayText,
                input.Description,
                new List<DataDictionaryItem>(),
                input.IsStatic);
            
            foreach (var itemDto in input.Items)
            {
                newDict.AddOrUpdateItem(itemDto.Code, itemDto.DisplayText, itemDto.Description);
            }

            await _dataDictionaryManager.CreateAsync(newDict);

            return ObjectMapper.Map<DataDictionary, DataDictionaryDto>(newDict);
        }

        public async Task<DataDictionaryDto> UpdateAsync(Guid id, DataDictionaryUpdateDto input)
        {
            var dict = await _dataDictionaryRepository.GetAsync(id);

            dict.SetContent(input.DisplayText, input.Description);
            
            foreach (var itemDto in input.Items)
            {
                dict.AddOrUpdateItem(itemDto.Code, itemDto.DisplayText, itemDto.Description);
            }

            dict.Items.RemoveAll(item => !input.Items.Select(dtoItem => dtoItem.Code).Contains(item.Code));

            await _dataDictionaryManager.UpdateAsync(dict);

            return ObjectMapper.Map<DataDictionary, DataDictionaryDto>(dict);
        }

        public Task DeleteAsync(Guid id) => _dataDictionaryRepository.DeleteAsync(id);

        public async Task<DataDictionaryDto> GetAsync(Guid id)
        {
            var dict = await _dataDictionaryRepository.GetAsync(id);
            return ObjectMapper.Map<DataDictionary, DataDictionaryDto>(dict);
        }

        public async Task<PagedResultDto<DataDictionaryDto>> GetListAsync(PagedResultRequestDto input)
        {
            var totalCount = await _dataDictionaryRepository.GetCountAsync();
            var resultList = await _dataDictionaryRepository.GetListAsync(input.SkipCount, input.MaxResultCount);
            return new PagedResultDto<DataDictionaryDto>(totalCount, ObjectMapper.Map<List<DataDictionary>, List<DataDictionaryDto>>(resultList));
        }
    }
}