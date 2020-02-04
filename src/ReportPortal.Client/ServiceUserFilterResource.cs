﻿using ReportPortal.Client.Abstractions;
using ReportPortal.Client.Abstractions.Filtering;
using ReportPortal.Client.Abstractions.Requests;
using ReportPortal.Client.Abstractions.Responses;
using ReportPortal.Client.Converters;
using ReportPortal.Client.Extentions;
using ReportPortal.Client.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReportPortal.Client
{
    public class ServiceUserFilterResource : BaseResource, IUserFilterResource
    {
        public ServiceUserFilterResource(HttpClient httpClient, Uri baseUri, string project, string apiToken) : base(httpClient, baseUri, project, apiToken)
        {
        }

        /// <summary>
        /// adds the specified user ilter
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<UserFilterCreatedResponse> CreateAsync(CreateUserFilterRequest model)
        {
            var uri = BaseUri.Append($"{Project}/filter");

            var body = ModelSerializer.Serialize<CreateUserFilterRequest>(model);
            var response = await HttpClient.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            response.VerifySuccessStatusCode();
            return ModelSerializer.Deserialize<UserFilterCreatedResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// gets all user filters
        /// </summary>
        /// <param name="filterOption"></param>
        /// <returns></returns>
        public virtual async Task<Content<UserFilter>> GetAsync(FilterOption filterOption = null)
        {
            var uri = BaseUri.Append($"{Project}/filter/");
            if (filterOption != null)
            {
                uri = uri.Append($"?{filterOption}");
            }
            var response = await HttpClient.GetAsync(uri).ConfigureAwait(false);
            response.VerifySuccessStatusCode();
            return ModelSerializer.Deserialize<Content<UserFilter>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public virtual async Task<UserFilter> GetAsync(long id)
        {
            var uri = BaseUri.Append($"{Project}/filter/{id}");

            var response = await HttpClient.GetAsync(uri).ConfigureAwait(false);
            response.VerifySuccessStatusCode();
            return ModelSerializer.Deserialize<UserFilter>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// deletes the specified filter by id
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
        public virtual async Task<Message> DeleteAsync(long id)
        {
            var uri = BaseUri.Append($"{Project}/filter/{id}");

            var response = await HttpClient.DeleteAsync(uri).ConfigureAwait(false);
            response.VerifySuccessStatusCode();
            return ModelSerializer.Deserialize<Message>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}