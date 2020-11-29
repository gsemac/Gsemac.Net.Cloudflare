(function () {

    function passWebDriverTest() {

        Object.defineProperty(window.navigator, 'webdriver', {
            get: () => undefined,
        });
        
    }

    function passAllTests() {

        passWebDriverTest();

    }

    passAllTests();

}());