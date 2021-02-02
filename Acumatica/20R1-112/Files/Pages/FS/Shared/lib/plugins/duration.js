Ext.define('Ext.ux.form.DurationField', {
    extend: 'Ext.form.field.Number', //Extending the NumberField
    alias: 'widget.durationfield', //Defining the xtype,
    currencySymbol: 'h',
    useThousandSeparator: false,
    thousandSeparator: ',',
    alwaysDisplayDecimals: false,
    fieldStyle: 'text-align: right;',
    initComponent: function() {
        if (this.useThousandSeparator && this.decimalSeparator == ',' && this.thousandSeparator == ',')
            this.thousandSeparator = '.';
        else
        if (this.allowDecimals && this.thousandSeparator == '.' && this.decimalSeparator == '.')
            this.decimalSeparator = ',';

        this.callParent(arguments);
    },
    setValue: function(value) {
        Ext.ux.form.DurationField.superclass.setValue.call(this, value != null ? value.toString().replace('.', this.decimalSeparator) : value);

        this.setRawValue(this.getFormattedValue(this.getValue()));
    },
    getFormattedValue: function(value) {
        if (Ext.isEmpty(value) || !this.hasFormat())
            return value;
        else {
            var neg = null;

            value = (neg = value < 0) ? value * -1 : value;
            value = this.allowDecimals && this.alwaysDisplayDecimals ? value.toFixed(this.decimalPrecision) : value;

            if (this.useThousandSeparator) {
                if (this.useThousandSeparator && Ext.isEmpty(this.thousandSeparator))
                    throw ('NumberFormatException: invalid thousandSeparator, property must has a valid character.');

                if (this.thousandSeparator == this.decimalSeparator)
                    throw ('NumberFormatException: invalid thousandSeparator, thousand separator must be different from decimalSeparator.');

                value = value.toString();

                var ps = value.split('.');
                ps[1] = ps[1] ? ps[1] : null;

                var whole = ps[0];

                var r = /(\d+)(\d{3})/;

                var ts = this.thousandSeparator;

                while (r.test(whole))
                    whole = whole.replace(r, '$1' + ts + '$2');

                value = whole + (ps[1] ? this.decimalSeparator + ps[1] : '');
            }

            return Ext.String.format('{0}{1}{2}', (neg ? '-' : ''), value, (Ext.isEmpty(this.currencySymbol) ? '' : ' ' + this.currencySymbol));
        }
    },

    getSubmitValue: function() {

        var me = this,
            value = me.callParent();

        if (!me.submitLocaleSeparator) {
            value = value.replace(me.decimalSeparator, '.');
        }
        return me.removeFormat(value);
    },

    /**
     * overrides parseValue to remove the format applied by this class
     */
    parseValue: function(value) {
        //Replace the currency symbol and thousand separator
        return Ext.ux.form.DurationField.superclass.parseValue.call(this, this.removeFormat(value));
    },
    /**
     * Remove only the format added by this class to let the superclass validate with it's rules.
     * @param {Object} value
     */
    removeFormat: function(value) {
        if (Ext.isEmpty(value) || !this.hasFormat())
            return value;
        else {
            value = value.toString().replace(' ' + this.currencySymbol, '');

            value = this.useThousandSeparator ? value.replace(new RegExp('[' + this.thousandSeparator + ']', 'g'), '') : value;

            return value;
        }
    },
    /**
     * Remove the format before validating the the value.
     * @param {Number} value
     */
    getErrors: function(value) {
        return Ext.ux.form.DurationField.superclass.getErrors.call(this, this.removeFormat(value));
    },
    hasFormat: function() {
        return this.decimalSeparator != '.' || (this.useThousandSeparator == true && this.getRawValue() != null) || !Ext.isEmpty(this.currencySymbol) || this.alwaysDisplayDecimals;
    },
    /**
     * Display the numeric value with the fixed decimal precision and without the format using the setRawValue, don't need to do a setValue because we don't want a double
     * formatting and process of the value because beforeBlur perform a getRawValue and then a setValue.
     */
    onFocus: function() {
        this.setRawValue(this.removeFormat(this.getRawValue()));

        this.callParent(arguments);
    }
});