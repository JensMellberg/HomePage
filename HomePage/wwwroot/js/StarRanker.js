class StarRanker {
	static create(callback) {
		const wrapper = HtmlUtils.createElement('div', 'star-wrapper');
		for (let i = 1; i < 11; i++) {
			const star = HtmlUtils.addElement('span', 'star', wrapper)
			$(star).click(() => callback(i))
			star.innerText = i.toString();
		}

		return wrapper
	}
}
