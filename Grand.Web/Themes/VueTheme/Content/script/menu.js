(function(){
    function $(selector, context){
        context = context || document;
        return context["querySelectorAll"](selector);
    }

    function forEach(collection, iterator){
        for(var key in Object.keys(collection)){
            iterator(collection[key]);
        }
    }

    function showMenu(menu){
        var menu = this;
        var ul = $("ul", menu)[0];

        if(!ul || ul.classList.contains("-visible")) return;

        ul.classList.add("-visible");
    }

    function hideMenu(menu){
        var menu = this;
        var ul = menu.parentElement;

        if(!ul || !ul.classList.contains("-visible")) return;

        setTimeout(function(){
            ul.classList.remove("-visible");
        }, 300);
    }

    window.addEventListener("load", function(){
        forEach($(".Menu li.-hasSubmenu"), function(e){
            e.showMenu = showMenu;
            e.hideMenu = hideMenu;
        });

        forEach($(".Menu > li.-hasSubmenu"), function(e){
            e.addEventListener("click", showMenu);
        });

        forEach($(".Menu li .back"), function(e){
            e.addEventListener("click", hideMenu);
        });

        forEach($(".Menu > li.-hasSubmenu li.-hasSubmenu"), function(e){
            e.addEventListener("click", showMenu);
        });
    });
})();
