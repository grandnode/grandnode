var PushNotifications = {
    url: "",
    SenderId: "",
    ApiKey: "",
    AuthDomain: "",
    DatabaseUrl: "",
    ProjectId: "",
    StorageBucket: "",

    init: function init(ApiKey, SenderId, ProjectId, AuthDomain, StorageBucket, DatabaseUrl, url) {
        this.url = url;
        this.SenderId = SenderId;
        this.ApiKey = ApiKey;
        this.AuthDomain = AuthDomain;
        this.DatabaseUrl = DatabaseUrl;
        this.ProjectId = ProjectId;
        this.StorageBucket = StorageBucket;
    },

    process: function process() {

        var config = {
            apiKey: this.ApiKey,
            authDomain: this.AuthDomain,
            databaseURL: this.DatabaseUrl,
            projectId: this.ProjectId,
            storageBucket: this.StorageBucket,
            messagingSenderId: this.SenderId
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

                        $.ajax({
                            cache: false,
                            url: url,
                            data: postData,
                            type: 'post'
                        });
                    });
            })
            .catch(function (err) {
                console.log('Unable to get permission to notify. ', err);

                var postData = {
                    success: false,
                    value: "Permission denied",
                };

                addAntiForgeryToken(postData);

                $.ajax({
                    cache: false,
                    url: url,
                    data: postData,
                    type: 'post'
                });
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