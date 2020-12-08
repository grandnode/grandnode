Vue.use(VeeValidate, {
    // This is the default
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
})