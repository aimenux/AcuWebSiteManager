// GmapPanel Configuration object 
var Cfg = {
    zoomLevel: 14,
    lat: 39.958438,
    lng: -105.033688,

    route_color: {
        lastPosition: 0,

        lineColors: [
            "#6A8080",
            "#6F6FA5",
            "#B82E00",
            "#990099",
            "#787845",
            "#556868",
            "#754719",
            "#0000FF",
            "#337733",
            "#009999",
            "#000000",
            "#195C9E",
            "#6F6F6F",
            "#901956",
            "#6600CC"
        ]
    },
    nodeTypes: {
        EMPLOYEE: 'EMPLOYEE'
    },
    markerColors: {
        START: '04B404',
        WAYPOINT: '00BFFF',
        END: 'DF0101',
        UNASSIGNED: 'A4A4A4'
    },
    googleChart: {
        baseUrl: 'https://chart.googleapis.com/chart?',
        pinShadow: '_withshadow',
        pinType: {
            letter: 'd_map_pin_letter', //pin with a character
            icon: 'd_map_pin_icon' // pin with an icon
        },
        icons: {
            glyphishEye: 'glyphish_eye'
        },
        getGoogleChartUrl: function(pinType, icon, pinCharacter, pinShadow, color) {

            if (pinType == null) {
                return false;
            }

            var url = Cfg.googleChart.baseUrl;

            url = url + 'chst=' + pinType;

            if (pinShadow) {
                url = url + Cfg.googleChart.pinShadow;
            }

            switch (pinType) {
                case (Cfg.googleChart.pinType.letter):
                    url = url + '&chld=' + pinCharacter;
                    break;
                case (Cfg.googleChart.pinType.icon):
                    url = url + '&chld=' + icon;
                    break;
                default:
                    break;
            }

            url = url + '|' + color;
            return url;
        }
    },
    getNextColor: function() {

        var color = null;

        color = Cfg.route_color.lineColors[Cfg.route_color.lastPosition];

        Cfg.route_color.lastPosition += 1;

        if (Cfg.route_color.lastPosition >= Cfg.route_color.lineColors.length - 1) {
            Cfg.route_color.lastPosition = 0;
        }

        return color;
    },

    getRandomColor: function() {
        var letters = '0123456789ABCDEF'.split('');
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }
}