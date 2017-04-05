## Prerequisited

### Visual Studio

You can develop with Visual Studio 2013 or greater, we usually use VS 2015. You need to configure package manager to point to myget repositories (these settings works both for *VS2013* and *VS2015*)

- Jarvis: *https://www.myget.org/F/jarvis/*
- NeventStore develop: *https://www.myget.org/F/neventstore-ci/api/v2*

### MongoDb

You can install MongoDb locally, remember that only starting with version 1.9 Jarvis is able to use Wired Tiger as persistence engine.

This is a typical configuration for Mongo with standard MMapV1

	systemLog:
	   quiet: false
	   logAppend: true
	storage:
	   engine: mmapv1
	   dbPath: "Z:/NoSql/Mongo3/data/"
	security:
	   authorization: disabled 

Here is a typical configuration that you can use for Wired Tiger.

	systemLog:
	   destination: file
	   path: "Z:/NoSql/Mongo3/logs/mongowt.log"
	   quiet: false
	   logAppend: true
	storage:
	   dbPath: "Z:/NoSql/Mongo3/datawiredtiger/"
	   engine: "wiredTiger"
	   wiredTiger.collectionConfig.blockCompressor: "snappy"
	security:
	   authorization: disabled

Remember that you should [enable authentication](http://www.codewrecks.com/blog/index.php/2016/05/21/grant-right-to-use-eval-on-mongodb-3-2/) to verify that Jarvis has no problem in accessing MongoDb with authentication.



