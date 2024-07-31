using Newtonsoft.Json.Linq;

namespace Rabobank.Compliancy.Core.Rules.Tests.Resources;

public static class EnvironmentChecks
{
    public static readonly JObject ValidateEnvironmentSingle = JObject.Parse(@"
{
  ""fps"": {
    ""dataProviders"": {
      ""data"": {
        ""ms.vss-pipelinechecks.checks-data-provider"": {
          ""checkConfigurationDataList"": [
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""Invoke Azure Function"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""true"",
                    ""function"": ""https://validategatesdev.azurewebsites.net/api/validate-yaml-approvers/arg1/arg2/"",
                    ""key"": ""anykey""
                  },
                  ""retryInterval"": ""0"",
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""97"",
                  ""name"": ""ValidateEnvironmentSingle""
                }
              }
            }
          ]
        }
      }
    }
  }
}");

    public static readonly JObject ValidateYamlApproversSingle = JObject.Parse(@"
{
  ""fps"": {
    ""dataProviders"": {
      ""data"": {
        ""ms.vss-pipelinechecks.checks-data-provider"": {
          ""checkConfigurationDataList"": [
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""4-eyes principle check"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""true"",
                    ""function"": ""https://validategatesdev.azurewebsites.net/api/validate-yaml-approvers/arg1/arg2/"",
                    ""key"": ""anykey""
                  },
                  ""retryInterval"": ""0"",
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""97"",
                  ""name"": ""ValidateEnvironmentSingle""
                }
              }
            }
          ]
        }
      }
    }
  }
}");

    public static readonly JObject ValidateYamlApproversWithMultiChecks = JObject.Parse(@"
{
  ""fps"": {
    ""dataProviders"": {
      ""data"": {
        ""ms.vss-pipelinechecks.checks-data-provider"": {
          ""checkConfigurationDataList"": [
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""4-eyes principle check"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""true"",
                    ""function"": ""https://validategatesdev.azurewebsites.net/api/validate-yaml-approvers/arg1/arg2/"",
                    ""key"": ""anykey""
                  },
                  ""retryInterval"": ""0"",
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""97"",
                  ""name"": ""ValidateEnvironmentSingle""
                }
              }
            },
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""4-eyes principle check"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""true"",
                    ""function"": ""https://validategatesdev.azurewebsites.net/api/validate-yaml-approvers/arg1/arg2/"",
                    ""key"": ""anykey""
                  },
                  ""retryInterval"": ""0"",
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""98"",
                  ""name"": ""ValidateEnvironmentSingle""
                }
              }
            },
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""Invoke azure function"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""true"",
                    ""function"": ""https://validategatesdev.azurewebsites.net/api/validate-yaml-approvers/arg1/arg2/"",
                    ""key"": ""anykey""
                  },
                  ""retryInterval"": ""0"",
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""99"",
                  ""name"": ""ValidateEnvironmentSingle""
                }
              }
            }
          ]
        }
      }
    }
  }
}");

    public static readonly JObject ValidateYamlApproversWithOtherChecks = JObject.Parse(@"
{
  ""fps"": {
    ""dataProviders"": {
      ""data"": {
        ""ms.vss-pipelinechecks.checks-data-provider"": {
          ""checkConfigurationDataList"": [
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""Invoke Azure Function"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""true"",
                    ""function"": ""https://validategatesprd.azurewebsites.net/api/validate-yaml-approvers/arg1/arg2/"",
                    ""key"": ""anykey""
                  },
                  ""retryInterval"": ""0"",
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""97"",
                  ""name"": ""ValidateEnvironmentWithOtherChecks""
                }
              }
            },
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""Invoke Azure Function"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""false"",
                    ""function"": ""https://test.azurewebsites.net/api/some-other-check/arg1/arg2/"",
                    ""key"": ""anykey""
                  }
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""97"",
                  ""name"": ""ValidateEnvironmentWithOtherChecks""
                }
              }
            }
          ]
        }
      }
    }
  }
}");

    public static readonly JObject InvalidValidateGatesUrl = JObject.Parse(@"
{
  ""fps"": {
    ""dataProviders"": {
      ""data"": {
        ""ms.vss-pipelinechecks.checks-data-provider"": {
          ""checkConfigurationDataList"": [
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""Invoke Azure Function"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""false"",
                    ""function"": ""https://test.azurewebsites.net/api/validate-env____ironment/arg1/arg2/"",
                    ""key"": ""anykey""
                  }
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""97"",
                  ""name"": ""MultiStagePipelineTestProd""
                }
              }
            }
          ]
        }
      }
    }
  }
}");

    public static readonly JObject WithoutCallBack = JObject.Parse(@"
{
  ""fps"": {
    ""dataProviders"": {
      ""data"": {
        ""ms.vss-pipelinechecks.checks-data-provider"": {
          ""checkConfigurationDataList"": [
            {
              ""definitionRefId"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
              ""checkConfiguration"": {
                ""settings"": {
                  ""displayName"": ""Invoke Azure Function"",
                  ""definitionRef"": {
                    ""id"": ""537fdb7a-a601-4537-aa70-92645a2b5ce4"",
                    ""name"": ""AzureFunction"",
                    ""version"": ""1.0.12""
                  },
                  ""inputs"": {
                    ""method"": ""POST"",
                    ""waitForCompletion"": ""false"",
                    ""function"": ""https://validategatesprd.azurewebsites.net/api/validate-yaml-approvers/arg1/arg2/"",
                    ""key"": ""anykey""
                  }
                },
                ""timeout"": 43200,
                ""id"": 87,
                ""type"": {
                  ""id"": ""fe1de3ee-a436-41b4-bb20-f6eb4cb879a7"",
                  ""name"": ""Task Check""
                },
                ""resource"": {
                  ""type"": ""environment"",
                  ""id"": ""97"",
                  ""name"": ""MultiStagePipelineTestProd""
                }
              }
            }
          ]
        }
      }
    }
  }
}");

    public static readonly JObject NoChecks = JObject.Parse(@"
{
  ""fps"": {
    ""dataProviders"": {
      ""data"": {
        ""ms.vss-pipelinechecks.checks-data-provider"": {
          ""checkConfigurationDataList"": []
        }
      }
    }
  }
}");
}