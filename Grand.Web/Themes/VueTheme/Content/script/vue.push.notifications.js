var PushNotifications = {
    url: "",
    SenderId: "",
    ApiKey: "",
    AuthDomain: "",
    DatabaseUrl: "",
    ProjectId: "",
    StorageBucket: "",
    AppId: "",

    init: function init(ApiKey, SenderId, ProjectId, AuthDomain, StorageBucket, DatabaseUrl, url, appId) {
        this.url = url;
        this.SenderId = SenderId;
        this.ApiKey = ApiKey;
        this.AuthDomain = AuthDomain;
        this.DatabaseUrl = DatabaseUrl;
        this.ProjectId = ProjectId;
        this.StorageBucket = StorageBucket;
        this.AppId = appId;
    },

    process: function process() {

        var config = {
            apiKey: this.ApiKey,
            authDomain: this.AuthDomain,
            databaseURL: this.DatabaseUrl,
            projectId: this.ProjectId,
            storageBucket: this.StorageBucket,
            messagingSenderId: this.SenderId,
            AppId: this.appId
        };

        firebase.initializeApp(config);

        const messaging = firebase.messaging();
        var url = this.url;

        messaging.requestPermission()
            .then(function () {
                var success = false;
                var value = "";

                messaging.getToken()
                    .then(function (currentToken) {
                        if (currentToken) {
                            success = true;
                            value = currentToken;
                        } else {
                            success = false;
                            value = 'No Instance ID token available. Request permission to generate one';
                        }
                    })
                    .catch(function (err) {
                        success = false;
                        value = 'An error occurred while retrieving token. ' + err;
                    })
                    .finally(function () {
                        
                        var postData = {
                            success: success,
                            value: value,
                        };

                        addAntiForgeryToken(postData);

                        axios({
                            url: url,
                            data: postData,
                            method: 'post',
                            headers: {
                                'Accept': 'application/json',
                                'Content-Type': 'application/json'
                            }
                        })
                    });
            })
            .catch(function (err) {
                console.log('Unable to get permission to notify. ', err);

                var postData = {
                    success: false,
                    value: "Permission denied",
                };

                addAntiForgeryToken(postData);

                axios({
                    url: url,
                    data: postData,
                    method: 'post',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    }
                })
            });

        messaging.onMessage(function (payload) {
            const notificationTitle = payload.notification.title;
            const notificationOptions = {
                body: payload.notification.body,
                icon: payload.notification.icon
            };

            var notification = new Notification(notificationTitle, notificationOptions);
        });
    }
}