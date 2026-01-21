var hasLongNodeList = false;
var companyStructureHandler = (function () {
    var $companyModal = $("#companyStucturalInfoModal");
    var downloading = false;
    
    function convert(arr) {
        var convertedArray = [];
        $(arr).each(function () {
            var found = false;
            for (var i = 0; i < convertedArray.length; i++) {
                if (convertedArray[i].id === "loc_"+this.LocationId) {
                    found = true;
                    convertedArray[i].children.push({
                        "id": "dep_"+this.DepartmentId,
                        "name": this.DepartmentName
                    });
                    break;
                }
            }
            if (!found) {
                convertedArray.push({
                    "id": "loc_"+this.LocationId,
                    "name": this.LocationName,
                    "children": [{
                        "id": "dep_"+this.DepartmentId,
                        "name": this.DepartmentName
                    }]
                });
            }
        });
        return convertedArray;
    }

    function getData() {
        var url = hdnBaseUrl + "Common/LocationDepartmentMapList"
        return new Promise((resolve, reject) => {
            $.ajax({
                url: url,
                type: 'POST',
                data: {
                    key: 'value',
                },
                success: function (data) {
                    var convertedMapArray = convert(data.map)
                    var mapJson = {
                        "id": 1,
                        "name": $('#structurecompanyname').html(),
                        children: convertedMapArray
                    }
                    resolve(mapJson);
                },
                error: function (error) {
                    reject(error)
                },
            })
        })
    }
    function showCompanyStructure() {
        $companyModal.modal('show');
    }

    var initialRootDataRequestPromise = getData();
    initialRootDataRequestPromise.then(function (result) {
        var options = {
            data: result,
            element: document.getElementById("visualisation"),
            allowFocus: true,
            allowZoom: false,
            allowPan: true,
            alowNodeCentering: false,
            getId: function (data) {
                return data.id;
            },
            getChildren: function (data) {
                return data.children;
            },
            getDisplayText: function (data) {
                return data.name;
            }
        };
        var treePlugin;
        if (result.children.length > 40) {
            hasLongNodeList = true;
        } else {
            for (var i = 0; i < result.children.length; i++) {
                if (result.children[i].children.length > 40) {
                    hasLongNodeList = true;
                    break;
                }
            }
        }

        if (hasLongNodeList) {
            treePlugin = new d3.mitchTree.circleTree(options)
                .getNodeSettings()
                .setSizingMode('nodesize')
                .setVerticalSpacing(25)
                .back()
                .initialize();
        } else {
            treePlugin = new d3.mitchTree.circleTree(options).initialize();
        }
        
        //var nodes = treePlugin.getNodes();

        //nodes.forEach(function (node, index, arr) {
        //    treePlugin.expand(node);
        //});
        //treePlugin.update(treePlugin.getRoot());
        //var treePlugin = new d3.mitchTree.boxedTree()
        //    .setData(result)
        //    .setElement(document.getElementById("visualisation"))
        //    .setIdAccessor(function (data) {
        //        return data.id;
        //    })
        //    .setChildrenAccessor(function (data) {
        //        return data.children;
        //    })
        //    .setBodyDisplayTextAccessor(function (data) {
        //        return data.description;
        //    })
        //    .setTitleDisplayTextAccessor(function (data) {
        //        return data.name;
        //    })
        //    .getLoadOnDemandSettings()
        //    .setLoadChildrenMethod(function (data, processData) {
        //        var nodeIdToLoadChildrenFor = this.getId(data);
        //        requestDirectChildrenData(nodeIdToLoadChildrenFor).then(function (result) {
        //            processData(result);
        //        }, function () {
        //            throw arguments;
        //        });
        //    })
        //    .setHasChildrenMethod(function (data) {
        //        return data.hasChildren;
        //    })
        //    .back()
        //    .initialize();
            //.setAllowFocus(false)
            //.setAllowZoom(false)
            //.setAllowPan(true)
            //.setAllowNodeCentering(false)
            //.setData(result)
            //.setElement(document.getElementById("visualisation"))
            //.setIdAccessor(function (data) {
            //    return data.id;
            //})
            //.setChildrenAccessor(function (data) {
            //    return data.children;
            //})
            //.setBodyDisplayTextAccessor(function (data) {
            //    return data.description;
            //})
            //.setTitleDisplayTextAccessor(function (data) {
            //    return data.name;
            //})
            //.initialize();
    });


    //download company structure
    function downloadStructure(e) {
        //e.preventDefault();
        if (!downloading) {
            downloading = true;
            var path = hdnBaseUrl + "Common/DownloadCompanyStructue";
            window.location = path;
            downloading = false;
        }
        return false;
    }

    return {
        showCompanyStructure: showCompanyStructure,
        downloadStructure: downloadStructure
    }
})()