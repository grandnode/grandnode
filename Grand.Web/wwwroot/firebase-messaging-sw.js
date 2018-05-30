importScripts('https://www.gstatic.com/firebasejs/4.13.0/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/4.13.0/firebase-messaging.js');

var config = {
apiKey: "AIzaSyAbFkgiamuSPPqd8nm2CxaeECzJMe0KGI4",
authDomain: "grandnode-push.firebaseapp.com",
databaseURL: "https://grandnode-push.firebaseio.com",
projectId: "grandnode-push",
storageBucket: "grandnode-push.appspot.com",
messagingSenderId: "810797720674",
};

firebase.initializeApp(config);

const messaging = firebase.messaging();

messaging.setBackgroundMessageHandler(function (payload) {
    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
        icon: payload.notification.icon
    };

    return self.registration.showNotification(notificationTitle,
        notificationOptions);
});
