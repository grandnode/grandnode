var delayTimer;
function autocompleteVue(e) {
    function getCategories(item) {
        if (item.SearchType == 'Category') {
            return item
        }
    }
    function getManufacturers(item) {
        if (item.SearchType == 'Manufacturer') {
            return item
        }
    }
    function getBlog(item) {
        if (item.SearchType == 'Blog') {
            return item
        }
    }
    function getProducts(item) {
        if (item.SearchType == 'Product') {
            return item
        }
    }
    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        if (e.checkValidity()) {
            var searchResult = document.getElementById('adv_search');
            if (searchResult) {
                if (e.value != '') {
                    searchResult.style.display = 'block';
                }
            }
            var value = e.value;
            var category = '';
            if (document.getElementById("SearchCategoryId")) {
                category = document.getElementById("SearchCategoryId").value;
            }
            axios({
                url: '/catalog/searchtermautocomplete',
                method: 'get',
                params: {
                    term: value,
                    categoryId: category
                }
            }).then(function (response) {
                if (response.data) {
                    vm.searchitems = response.data;
                    var categories = response.data.map(getCategories).filter(function (element) {
                        return element !== undefined;
                    });
                    var manufacturers = response.data.map(getManufacturers).filter(function (element) {
                        return element !== undefined;
                    });
                    var blog = response.data.map(getBlog).filter(function (element) {
                        return element !== undefined;
                    });
                    var products = response.data.map(getProducts).filter(function (element) {
                        return element !== undefined;
                    });
                    vm.searchcategories = categories;
                    vm.searchmanufacturers = manufacturers;
                    vm.searchblog = blog;
                    vm.searchproducts = products;
                }
            })
        } else {
            var searchResult = document.getElementById('adv_search');
            if (searchResult) {
                searchResult.style.display = 'none';
            }
        }
    }, 600);
}