﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebSocketDemo.Models;

namespace WebSocketDemo.Services
{
    public class HashService : QueueProcessor<HashRequest>
    {
        public HashService(IProducerConsumerCollection<HashRequest> workQueue, ILoggerFactory loggerFactory)
            : base(workQueue, loggerFactory.CreateLogger<HashService>()) { }

        protected override async Task HandleRequest(HashRequest request, CancellationToken cancelToken)
        {
            request.Job.Status = JobStatus.Pending;

            await Task.Delay(TimeSpan.FromSeconds(6));

            request.Job.Result = new HashResult
            {
                Md5 = Hash(request.Data, MD5.Create),
                Sha1 = Hash(request.Data, SHA1.Create),
                Sha256 = Hash(request.Data, SHA256.Create),
                Sha512 = Hash(request.Data, SHA512.Create),
            };
            request.Job.Status = JobStatus.Complete;
        }

        static string Hash(Stream stream, Func<HashAlgorithm> hasherFactory)
        {
            var result = GetHexString(CalculateHash(stream, hasherFactory));
            stream.Position = 0;
            return result;
        }

        static byte[] CalculateHash(Stream stream, Func<HashAlgorithm> hasherFactory)
        {
            using (var hasher = hasherFactory())
            {
                return hasher.ComputeHash(stream);
            }
        }

        static string GetHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
