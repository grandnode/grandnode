const config = {
    inject: true,
    // Important to name this something other than 'fields'
    fieldsBagName: 'veeFields',
    // This is not required but avoids possible naming conflicts
    errorBagName: 'veeErrors',
    classes: true,
    classNames: {
        valid: 'is-valid',
        invalid: 'is-invalid'
    }
}
Vue.use(VeeValidate, config);
const vee = new VeeValidate(config, Vue);

function vee_getMessage(field, rule) {
    var element = document.getElementsByName(field);
    if (element && element[0]) {
        var text = element[0].getAttribute('data-val-' + rule);
        if (text)
            return text;
    }
}

vee._validator.extend('required', {
    getMessage: field => {
        var text = vee_getMessage(field, 'required');
        if (text) {
            return text;
        }
        return 'The ' + field + ' field is required.'
    },
    validate: value => !!value
});

vee._validator.extend('email', {
    getMessage: field => {
        var text = vee_getMessage(field, 'email');
        if (text) {
            return text;
        }
        return 'This field must be a valid email.'
    },
    validate: value => {
        // if the field is empty
        if (!value) {
            return true;
        }
        // if the field is not a valid email
        if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$/i.test(value)) {
            return false;
        }
        // All is good
        return true;
    }
});

vee._validator.extend('confirmed', {
    paramNames: ['targetValue'],
    validate: function (value, ref) {
        return value === ref.targetValue;
    },
    options: {
        hasTarget: true
    },
    getMessage: field => {
        var text = vee_getMessage(field, 'password');
        if (text) {
            return text;
        }
        return 'Password confirmation does not match'
    },
});

vee._validator.extend('min', {
    getMessage: field => {
        var text = vee_getMessage(field, 'length');
        if (text) {
            return text;
        }
        return 'This ' + field + ' should have at least  characters.'
    },
    options: {
        hasTarget: true
    },
    paramNames: ['targetValue'],
    validate: function (value, ref) {
        // if the field is empty
        if (!value) {
            return true;
        }
        var element = document.getElementsByName(ref.targetValue);
        var number = parseInt(element[0].getAttribute('data-val-length-min'));
        var length = value.length;
        if (length < number) {
            return false;
        }
        // All is good
        return true;
    }
});