FROM golang:1.10
ENV USER decred
RUN adduser --disabled-password --gecos ''  $USER

RUN go get -u -v github.com/golang/dep/cmd/dep
RUN git clone https://github.com/decred/dcrdata $GOPATH/src/github.com/decred/dcrdata
RUN cd $GOPATH/src/github.com/decred/dcrdata && \
    dep ensure && \
go install

USER $USER
WORKDIR $GOPATH/src/github.com/decred/dcrdata