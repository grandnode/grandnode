
//Update fields AppliedDiscounts in the Store Collection 
db.Store.find({ AppliedDiscounts: { $exists: true }, $where: 'this.AppliedDiscounts.length > 0' }).forEach(function (store) {
	if (store.AppliedDiscounts[0]["_id"] != null) {
		var discounts = store.AppliedDiscounts;
		store.AppliedDiscounts = [];
		for (var i = 0; i < discounts.length; ++i) {
			var discount = discounts[i];
			var discountId = discount["_id"];
			store.AppliedDiscounts.push(discountId);
		}
		db.Store.save(store);
	}
});


//Update fields AppliedDiscounts in the Category Collection 
db.Category.find({AppliedDiscounts:{$exists:true}, $where:'this.AppliedDiscounts.length > 0'}).forEach( function(category) {
	if (category.AppliedDiscounts[0]["_id"] != null) {
		var discounts = category.AppliedDiscounts;
		category.AppliedDiscounts = [];
		for (var i = 0; i < discounts.length; ++i) {
			var discount = discounts[i];
			var discountId = discount["_id"];
			category.AppliedDiscounts.push(discountId);
		}
		db.Category.save(category);
	}
});

//Update fields AppliedDiscounts in the Manufacturer Collection 
db.Manufacturer.find({ AppliedDiscounts: { $exists: true }, $where: 'this.AppliedDiscounts.length > 0' }).forEach(function (manufacturer) {
	if (manufacturer.AppliedDiscounts[0]["_id"] != null) {
		var discounts = manufacturer.AppliedDiscounts;
		manufacturer.AppliedDiscounts = [];
		for (var i = 0; i < discounts.length; ++i) {
			var discount = discounts[i];
			var discountId = discount["_id"];
			manufacturer.AppliedDiscounts.push(discountId);
		}
		db.Manufacturer.save(manufacturer);
	}
});

//Update fields AppliedDiscounts in the Vendor Collection 
db.Vendor.find({ AppliedDiscounts: { $exists: true }, $where: 'this.AppliedDiscounts.length > 0' }).forEach(function (vendor) {
	if (vendor.AppliedDiscounts[0]["_id"] != null) {
		var discounts = vendor.AppliedDiscounts;
		vendor.AppliedDiscounts = [];
		for (var i = 0; i < discounts.length; ++i) {
			var discount = discounts[i];
			var discountId = discount["_id"];
			vendor.AppliedDiscounts.push(discountId);
		}
		db.Vendor.save(vendor);
	}
});


//Update fields AppliedDiscounts in the Product Collection 
db.Product.find({ AppliedDiscounts: { $exists: true }, $where: 'this.AppliedDiscounts.length > 0' }).forEach(function (product) {
	if (product.AppliedDiscounts[0]["_id"] != null) {
		var discounts = product.AppliedDiscounts;
		product.AppliedDiscounts = [];
		for (var i = 0; i < discounts.length; ++i) {
			var discount = discounts[i];
			var discountId = discount["_id"];
			product.AppliedDiscounts.push(discountId);
		}
		db.Product.save(product);
	}
});

//remove field HasDiscountsApplied from the Product Collection
db.Product.update({}, { $unset: { HasDiscountsApplied: 1 } }, { multi: true });

//Update fields ProductTags in the Product Collection 
db.Product.find({ProductTags:{$exists:true}, $where:'this.ProductTags.length > 0'}).forEach( function(product) {
	if (product.ProductTags[0]["_id"] != null) {
		var tags = product.ProductTags;
		product.ProductTags = [];
		for (var i = 0; i < tags.length; ++i) {
			var tag = tags[i];
			var tagId = tag["_id"];
			product.ProductTags.push(tagId);
		}
		db.Product.save(product);
	}
});

//Update email addresses and UserName to lowercase - required changes
db.Customer.find().forEach(
  function (e) {
      if (e.Email != null)
          e.Email = e.Email.toLowerCase();
      if (e.Username != null)
          e.Username = e.Username.toLowerCase();
      if (e.Email != null || e.Username != null)
          db.Customer.save(e);
  }
)

print("Update executed");